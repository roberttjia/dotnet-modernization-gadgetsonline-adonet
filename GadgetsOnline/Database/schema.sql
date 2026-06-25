-- =============================================================
-- GadgetsOnline - Schema (DDL)
-- Target: SQL Server. Run against the GadgetsOnline database.
-- Tables are created only if they do not already exist so the
-- script is safe to run repeatedly on startup.
-- =============================================================

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryId   INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        Name         NVARCHAR(255)   NOT NULL,
        Description  NVARCHAR(1024)  NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId      INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        CategoryId     INT             NOT NULL,
        Name           NVARCHAR(255)   NOT NULL,
        Price          DECIMAL(18,2)   NOT NULL,
        ProductArtUrl  NVARCHAR(1024)  NULL,
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories (CategoryId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Carts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts
    (
        RecordId     INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        CartId       NVARCHAR(255)   NOT NULL,
        ProductId    INT             NOT NULL,
        Count        INT             NOT NULL,
        DateCreated  DATETIME        NOT NULL,
        CONSTRAINT FK_Carts_Products FOREIGN KEY (ProductId)
            REFERENCES dbo.Products (ProductId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId     INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        OrderDate   DATETIME        NOT NULL,
        Username    NVARCHAR(255)   NULL,
        FirstName   NVARCHAR(160)   NOT NULL,
        LastName    NVARCHAR(160)   NOT NULL,
        Address     NVARCHAR(70)    NOT NULL,
        City        NVARCHAR(40)    NOT NULL,
        State       NVARCHAR(40)    NOT NULL,
        PostalCode  NVARCHAR(10)    NOT NULL,
        Country     NVARCHAR(40)    NOT NULL,
        Phone       NVARCHAR(24)    NOT NULL,
        Email       NVARCHAR(255)   NOT NULL,
        Total       DECIMAL(18,2)   NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDetails
    (
        OrderDetailId  INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        OrderId        INT            NOT NULL,
        ProductId      INT            NOT NULL,
        Quantity       INT            NOT NULL,
        UnitPrice      DECIMAL(18,2)  NOT NULL,
        CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders (OrderId),
        CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId)
            REFERENCES dbo.Products (ProductId)
    );
END;
GO
