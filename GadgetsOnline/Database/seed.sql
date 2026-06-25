-- =============================================================
-- GadgetsOnline - Seed data
-- Mirrors the data previously created by GadgetsOnlineInitializer.
-- Only seeds when the Categories table is empty so it is safe to
-- run on every startup.
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    -- Insert categories with explicit ids so product foreign keys line up.
    SET IDENTITY_INSERT dbo.Categories ON;

    INSERT INTO dbo.Categories (CategoryId, Name, Description) VALUES
        (1, N'Mobile Phones', N'Latest collection of Mobile Phones'),
        (2, N'Laptops',       N'Latest Laptops in 2022'),
        (3, N'Desktops',      N'Latest Desktops in 2022'),
        (4, N'Audio',         N'Latest audio devices'),
        (5, N'Accessories',   N'USB Cables, Mobile chargers and Keyboards etc');

    SET IDENTITY_INSERT dbo.Categories OFF;

    SET IDENTITY_INSERT dbo.Products ON;

    INSERT INTO dbo.Products (ProductId, CategoryId, Name, Price, ProductArtUrl) VALUES
        (1,  1, N'Phone 12',                      699.00,  N'/Content/Images/Mobile/1.jpg'),
        (2,  1, N'Phone 13 Pro',                  999.00,  N'/Content/Images/Mobile/2.jpg'),
        (3,  1, N'Phone 13 Pro Max',              1199.00, N'/Content/Images/Mobile/3.jpg'),
        (4,  2, N'XTS 13''',                       899.00,  N'/Content/Images/Laptop/1.jpg'),
        (5,  2, N'PC 15.5''',                      479.00,  N'/Content/Images/Laptop/2.jpg'),
        (6,  2, N'Notebook 14',                   169.00,  N'/Content/Images/Laptop/3.jpg'),
        (7,  3, N'The IdeaCenter',                539.00,  N'/Content/Images/placeholder.gif'),
        (8,  3, N'COMP 22-df003w',                389.00,  N'/Content/Images/placeholder.gif'),
        (9,  4, N'Bluetooth Headphones Over Ear', 28.00,   N'/Content/Images/Headphones/1.png'),
        (10, 4, N'ZX Series ',                    10.00,   N'/Content/Images/Headphones/2.png'),
        (11, 5, N'Wireless charger',              9.99,    N'/Content/Images/placeholder.gif'),
        (12, 5, N'Mousepad',                      2.99,    N'/Content/Images/placeholder.gif'),
        (13, 5, N'Keyboard',                      9.99,    N'/Content/Images/placeholder.gif');

    SET IDENTITY_INSERT dbo.Products OFF;
END;
GO

-- =============================================================
-- Additional products
-- This block is independent of the initial seed guard above so it
-- also applies to databases that were already seeded. Each product
-- is inserted only if a product with the same Id is not present,
-- making the block safe to re-run on every startup.
-- =============================================================

SET IDENTITY_INSERT dbo.Products ON;

INSERT INTO dbo.Products (ProductId, CategoryId, Name, Price, ProductArtUrl)
SELECT v.ProductId, v.CategoryId, v.Name, v.Price, v.ProductArtUrl
FROM (VALUES
    (14, 1, N'Phone 14',                  799.00,  N'/Content/Images/placeholder.gif'),
    (15, 1, N'Phone 14 Pro Max',          1299.00, N'/Content/Images/placeholder.gif'),
    (16, 1, N'Compact Phone SE',          429.00,  N'/Content/Images/placeholder.gif'),
    (17, 2, N'UltraBook Pro 16',          1899.00, N'/Content/Images/placeholder.gif'),
    (18, 2, N'Gaming Laptop GX',          1599.00, N'/Content/Images/placeholder.gif'),
    (19, 2, N'Budget Laptop 14',          349.00,  N'/Content/Images/placeholder.gif'),
    (20, 3, N'Tower Desktop Pro',         1099.00, N'/Content/Images/placeholder.gif'),
    (21, 3, N'All-in-One 27',             899.00,  N'/Content/Images/placeholder.gif'),
    (22, 3, N'Mini PC Cube',              459.00,  N'/Content/Images/placeholder.gif'),
    (23, 4, N'Noise Cancelling Earbuds',  149.00,  N'/Content/Images/placeholder.gif'),
    (24, 4, N'Studio Headphones Pro',     249.00,  N'/Content/Images/placeholder.gif'),
    (25, 4, N'Portable Bluetooth Speaker', 79.00,  N'/Content/Images/placeholder.gif'),
    (26, 5, N'USB-C Cable 2m',            12.99,   N'/Content/Images/placeholder.gif'),
    (27, 5, N'65W GaN Charger',           39.99,   N'/Content/Images/placeholder.gif'),
    (28, 5, N'Wireless Mouse',            24.99,   N'/Content/Images/placeholder.gif'),
    (29, 5, N'Mechanical Keyboard RGB',   89.99,   N'/Content/Images/placeholder.gif'),
    (30, 5, N'Laptop Stand Aluminium',    34.99,   N'/Content/Images/placeholder.gif')
) AS v(ProductId, CategoryId, Name, Price, ProductArtUrl)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Products p WHERE p.ProductId = v.ProductId
);

SET IDENTITY_INSERT dbo.Products OFF;
GO

-- =============================================================
-- Sample orders (DEMO DATA)
-- These exist so customer-facing features that read purchase
-- history (e.g. "frequently bought together") have data to work
-- with. There is no login in this app, so Username is 'Anonymous'
-- and buyer details are sample values. Guarded on OrderId = 1 so
-- the block is safe to re-run and does not duplicate.
-- Remove this block if you want a clean orders table.
-- =============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE OrderId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Orders ON;

    INSERT INTO dbo.Orders
        (OrderId, OrderDate, Username, FirstName, LastName, Address, City, State, PostalCode, Country, Phone, Email, Total)
    VALUES
        (1,  '2024-01-05', N'Anonymous', N'Ada',    N'Lovelace', N'1 Analytical Way', N'London',   N'LDN', N'10001', N'UK',  N'555-0101', N'ada@example.com',    751.98),
        (2,  '2024-01-06', N'Anonymous', N'Alan',   N'Turing',   N'2 Enigma Rd',      N'Manchester',N'MAN', N'10002', N'UK',  N'555-0102', N'alan@example.com',   857.99),
        (3,  '2024-01-07', N'Anonymous', N'Grace',  N'Hopper',   N'3 Compiler Ave',   N'New York', N'NY',  N'10003', N'USA', N'555-0103', N'grace@example.com',  1011.99),
        (4,  '2024-01-08', N'Anonymous', N'Linus',  N'Torvalds', N'4 Kernel St',      N'Portland', N'OR',  N'10004', N'USA', N'555-0104', N'linus@example.com',  1958.98),
        (5,  '2024-01-09', N'Anonymous', N'Margaret',N'Hamilton',N'5 Apollo Blvd',    N'Boston',   N'MA',  N'10005', N'USA', N'555-0105', N'margaret@example.com',1923.99),
        (6,  '2024-01-10', N'Anonymous', N'Dennis', N'Ritchie',  N'6 Unix Ln',        N'Murray Hill',N'NJ',N'10006', N'USA', N'555-0106', N'dennis@example.com', 1713.98),
        (7,  '2024-01-11', N'Anonymous', N'Ken',    N'Thompson', N'7 Plan9 Ct',       N'Murray Hill',N'NJ',N'10007', N'USA', N'555-0107', N'ken@example.com',    738.99),
        (8,  '2024-01-12', N'Anonymous', N'Barbara',N'Liskov',   N'8 Substitution Sq',N'Cambridge',N'MA',  N'10008', N'USA', N'555-0108', N'barbara@example.com',960.99),
        (9,  '2024-01-13', N'Anonymous', N'Donald', N'Knuth',    N'9 Algorithm Dr',   N'Stanford', N'CA',  N'10009', N'USA', N'555-0109', N'donald@example.com', 328.00),
        (10, '2024-01-14', N'Anonymous', N'Tim',    N'Berners-Lee',N'10 Web Way',     N'Geneva',   N'GVA', N'10010', N'CH',  N'555-0110', N'tim@example.com',    711.99);

    SET IDENTITY_INSERT dbo.Orders OFF;

    -- Order line items. UnitPrice mirrors the product price at purchase time.
    INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
        (1,  1,  1, 699.00), (1,  26, 1, 12.99), (1,  27, 1, 39.99),
        (2,  1,  1, 699.00), (2,  11, 1, 9.99),  (2,  23, 1, 149.00),
        (3,  2,  1, 999.00), (3,  26, 1, 12.99),
        (4,  17, 1, 1899.00),(4,  28, 1, 24.99), (4,  30, 1, 34.99),
        (5,  17, 1, 1899.00),(5,  28, 1, 24.99),
        (6,  18, 1, 1599.00),(6,  29, 1, 89.99), (6,  28, 1, 24.99),
        (7,  1,  1, 699.00), (7,  27, 1, 39.99),
        (8,  14, 1, 799.00), (8,  26, 1, 12.99), (8,  23, 1, 149.00),
        (9,  24, 1, 249.00), (9,  25, 1, 79.00),
        (10, 1,  1, 699.00), (10, 26, 1, 12.99);
END;
GO
