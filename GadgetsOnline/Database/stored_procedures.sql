-- =============================================================
-- GadgetsOnline - Stored Procedures
-- All data access for the application goes through these procs.
-- CREATE OR ALTER keeps the script idempotent.
-- =============================================================

-- ---------------------------------------------------------------
-- Categories
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.Categories_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, Name, Description
    FROM dbo.Categories
    ORDER BY CategoryId;
END;
GO

-- ---------------------------------------------------------------
-- Products
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.Products_GetBestSellers
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Count)
        p.ProductId, p.CategoryId, p.Name, p.Price, p.ProductArtUrl
    FROM dbo.Products p
    ORDER BY p.ProductId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Products_GetByCategoryName
    @CategoryName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProductId, p.CategoryId, p.Name, p.Price, p.ProductArtUrl
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
    WHERE c.Name = @CategoryName
    ORDER BY p.ProductId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Products_GetById
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Join to Categories so the data layer can hydrate Product.Category
    SELECT
        p.ProductId, p.CategoryId, p.Name, p.Price, p.ProductArtUrl,
        c.CategoryId   AS Category_CategoryId,
        c.Name         AS Category_Name,
        c.Description  AS Category_Description
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
    WHERE p.ProductId = @ProductId;
END;
GO

-- ---------------------------------------------------------------
-- Carts
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.Carts_GetByCartId
    @CartId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    -- Join to Products so the data layer can hydrate Cart.Product
    SELECT
        ct.RecordId, ct.CartId, ct.ProductId, ct.Count, ct.DateCreated,
        p.ProductId      AS Product_ProductId,
        p.CategoryId     AS Product_CategoryId,
        p.Name           AS Product_Name,
        p.Price          AS Product_Price,
        p.ProductArtUrl  AS Product_ProductArtUrl
    FROM dbo.Carts ct
    INNER JOIN dbo.Products p ON p.ProductId = ct.ProductId
    WHERE ct.CartId = @CartId
    ORDER BY ct.RecordId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Carts_GetItem
    @CartId NVARCHAR(255),
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RecordId, CartId, ProductId, Count, DateCreated
    FROM dbo.Carts
    WHERE CartId = @CartId AND ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Carts_AddItem
    @CartId NVARCHAR(255),
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Carts WHERE CartId = @CartId AND ProductId = @ProductId)
    BEGIN
        UPDATE dbo.Carts
        SET Count = Count + 1
        WHERE CartId = @CartId AND ProductId = @ProductId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Carts (CartId, ProductId, Count, DateCreated)
        VALUES (@CartId, @ProductId, 1, GETDATE());
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.Carts_RemoveItem
    @CartId NVARCHAR(255),
    @ProductId INT,
    @RemainingCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Count INT;

    SELECT @Count = Count
    FROM dbo.Carts
    WHERE CartId = @CartId AND ProductId = @ProductId;

    IF @Count IS NULL
    BEGIN
        SET @RemainingCount = 0;
        RETURN;
    END

    IF @Count > 1
    BEGIN
        UPDATE dbo.Carts
        SET Count = Count - 1
        WHERE CartId = @CartId AND ProductId = @ProductId;
        SET @RemainingCount = @Count - 1;
    END
    ELSE
    BEGIN
        DELETE FROM dbo.Carts
        WHERE CartId = @CartId AND ProductId = @ProductId;
        SET @RemainingCount = 0;
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.Carts_GetCount
    @CartId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(SUM(Count), 0) AS ItemCount
    FROM dbo.Carts
    WHERE CartId = @CartId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Carts_GetTotal
    @CartId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(SUM(ct.Count * p.Price), 0) AS CartTotal
    FROM dbo.Carts ct
    INNER JOIN dbo.Products p ON p.ProductId = ct.ProductId
    WHERE ct.CartId = @CartId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Carts_EmptyCart
    @CartId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Carts WHERE CartId = @CartId;
END;
GO

-- ---------------------------------------------------------------
-- Orders
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.Orders_Create
    @OrderDate  DATETIME,
    @Username   NVARCHAR(255),
    @FirstName  NVARCHAR(160),
    @LastName   NVARCHAR(160),
    @Address    NVARCHAR(70),
    @City       NVARCHAR(40),
    @State      NVARCHAR(40),
    @PostalCode NVARCHAR(10),
    @Country    NVARCHAR(40),
    @Phone      NVARCHAR(24),
    @Email      NVARCHAR(255),
    @Total      DECIMAL(18,2),
    @OrderId    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Orders
        (OrderDate, Username, FirstName, LastName, Address, City, State,
         PostalCode, Country, Phone, Email, Total)
    VALUES
        (@OrderDate, @Username, @FirstName, @LastName, @Address, @City, @State,
         @PostalCode, @Country, @Phone, @Email, @Total);

    SET @OrderId = CAST(SCOPE_IDENTITY() AS INT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.Orders_UpdateTotal
    @OrderId INT,
    @Total DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Orders SET Total = @Total WHERE OrderId = @OrderId;
END;
GO

-- ---------------------------------------------------------------
-- OrderDetails
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.OrderDetails_Add
    @OrderId   INT,
    @ProductId INT,
    @Quantity  INT,
    @UnitPrice DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);
END;
GO

-- ---------------------------------------------------------------
-- Recommendations: Frequently Bought Together
-- Given a product, find other products that appear in the same
-- orders, ranked by how often they co-occur. Pure read over
-- OrderDetails (self-join) + Products. Customer-facing; shown on
-- the product Details page.
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.Products_GetFrequentlyBoughtTogether
    @ProductId INT,
    @Count INT = 4
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        p.ProductId,
        p.CategoryId,
        p.Name,
        p.Price,
        p.ProductArtUrl,
        COUNT(DISTINCT od_other.OrderId) AS TimesBoughtTogether
    FROM dbo.OrderDetails od_target
    INNER JOIN dbo.OrderDetails od_other
        ON od_other.OrderId = od_target.OrderId
       AND od_other.ProductId <> od_target.ProductId
    INNER JOIN dbo.Products p
        ON p.ProductId = od_other.ProductId
    WHERE od_target.ProductId = @ProductId
    GROUP BY p.ProductId, p.CategoryId, p.Name, p.Price, p.ProductArtUrl
    ORDER BY TimesBoughtTogether DESC, p.Name ASC;
END;
GO

-- ---------------------------------------------------------------
-- Checkout: Place Order (atomic)
-- Consolidates the whole checkout into one server-side transaction:
--   1. validates the cart is not empty
--   2. computes the order total from the cart
--   3. inserts the order header
--   4. inserts all order line items in one set-based INSERT...SELECT
--   5. empties the cart
-- All steps commit together or roll back together. Returns the new
-- OrderId via an OUTPUT parameter.
-- ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.Checkout_PlaceOrder
    @CartId     NVARCHAR(255),
    @OrderDate  DATETIME,
    @Username   NVARCHAR(255),
    @FirstName  NVARCHAR(160),
    @LastName   NVARCHAR(160),
    @Address    NVARCHAR(70),
    @City       NVARCHAR(40),
    @State      NVARCHAR(40),
    @PostalCode NVARCHAR(10),
    @Country    NVARCHAR(40),
    @Phone      NVARCHAR(24),
    @Email      NVARCHAR(255),
    @OrderId    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OrderTotal DECIMAL(18,2);

    -- Guard: refuse to place an order for an empty cart.
    IF NOT EXISTS (SELECT 1 FROM dbo.Carts WHERE CartId = @CartId)
    BEGIN
        THROW 50001, 'Cannot place an order: the cart is empty.', 1;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Compute the total from the current cart contents.
        SELECT @OrderTotal = SUM(ct.Count * p.Price)
        FROM dbo.Carts ct
        INNER JOIN dbo.Products p ON p.ProductId = ct.ProductId
        WHERE ct.CartId = @CartId;

        -- Insert the order header with the computed total.
        INSERT INTO dbo.Orders
            (OrderDate, Username, FirstName, LastName, Address, City, State,
             PostalCode, Country, Phone, Email, Total)
        VALUES
            (@OrderDate, @Username, @FirstName, @LastName, @Address, @City, @State,
             @PostalCode, @Country, @Phone, @Email, @OrderTotal);

        SET @OrderId = CAST(SCOPE_IDENTITY() AS INT);

        -- Copy each cart line into OrderDetails, capturing the price at
        -- purchase time.
        INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
        SELECT @OrderId, ct.ProductId, ct.Count, p.Price
        FROM dbo.Carts ct
        INNER JOIN dbo.Products p ON p.ProductId = ct.ProductId
        WHERE ct.CartId = @CartId;

        -- Empty the cart now that it has been converted to an order.
        DELETE FROM dbo.Carts WHERE CartId = @CartId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;  -- re-raise so the caller sees the failure
    END CATCH
END;
GO

-- =============================================================
-- Reporting procedures (consumed by the GadgetsOnline.Reporting app)
-- Read-only analytics over Orders / OrderDetails / Products /
-- Categories. The store app creates these on startup; the reporting
-- app only calls them.
-- =============================================================

-- Sales aggregated by product category.
CREATE OR ALTER PROCEDURE dbo.Report_SalesByCategory
    @StartDate DATETIME = NULL,
    @EndDate   DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.CategoryId,
        c.Name AS CategoryName,
        COUNT(DISTINCT o.OrderId)        AS OrderCount,
        ISNULL(SUM(od.Quantity), 0)      AS UnitsSold,
        ISNULL(SUM(od.Quantity * od.UnitPrice), 0) AS Revenue
    FROM dbo.Categories c
    LEFT JOIN dbo.Products p     ON p.CategoryId = c.CategoryId
    LEFT JOIN dbo.OrderDetails od ON od.ProductId = p.ProductId
    LEFT JOIN dbo.Orders o        ON o.OrderId = od.OrderId
        AND (@StartDate IS NULL OR o.OrderDate >= @StartDate)
        AND (@EndDate   IS NULL OR o.OrderDate <  @EndDate)
    GROUP BY c.CategoryId, c.Name
    ORDER BY Revenue DESC, c.Name ASC;
END;
GO

-- Top selling products ranked by units sold, with a dense rank column.
CREATE OR ALTER PROCEDURE dbo.Report_TopSellingProducts
    @Count INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        p.ProductId,
        p.Name AS ProductName,
        c.Name AS CategoryName,
        SUM(od.Quantity)                  AS UnitsSold,
        SUM(od.Quantity * od.UnitPrice)   AS Revenue,
        RANK() OVER (ORDER BY SUM(od.Quantity) DESC) AS SalesRank
    FROM dbo.OrderDetails od
    INNER JOIN dbo.Products p   ON p.ProductId = od.ProductId
    INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
    GROUP BY p.ProductId, p.Name, c.Name
    ORDER BY UnitsSold DESC, p.Name ASC;
END;
GO

-- Monthly revenue with a running cumulative total (window function).
CREATE OR ALTER PROCEDURE dbo.Report_RevenueByMonth
AS
BEGIN
    SET NOCOUNT ON;

    WITH MonthlyTotals AS
    (
        SELECT
            DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1) AS MonthStart,
            COUNT(*)     AS OrderCount,
            SUM(o.Total) AS Revenue
        FROM dbo.Orders o
        GROUP BY DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1)
    )
    SELECT
        MonthStart,
        OrderCount,
        Revenue,
        SUM(Revenue) OVER (ORDER BY MonthStart
                           ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS RunningTotal
    FROM MonthlyTotals
    ORDER BY MonthStart;
END;
GO
