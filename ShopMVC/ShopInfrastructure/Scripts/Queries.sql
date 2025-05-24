-- Query 1: Find products that haven't been ordered yet and have quantity less than specified
CREATE OR ALTER PROCEDURE GetUnorderedProductsByQuantity
    @MaxQuantity INT
AS
BEGIN
    SELECT p.PdName, p.PdQuantity, p.PdPrice, m.MnName as Manufacturer
    FROM Products p
    JOIN Manufacturers m ON p.ManufacturerId = m.Id
    WHERE p.PdQuantity < @MaxQuantity
    AND NOT EXISTS (
        SELECT 1 FROM ProductOrders po WHERE po.ProductId = p.Id
    );
END;

-- Query 2: Find manufacturers that supply products ordered by a specific user
CREATE OR ALTER PROCEDURE GetManufacturersByUserOrders
    @UserId NVARCHAR(450)
AS
BEGIN
    SELECT DISTINCT m.MnName,
           STRING_AGG(p.PdName, ', ') as Products
    FROM Manufacturers m
    JOIN Products p ON m.Id = p.ManufacturerId
    JOIN ProductOrders po ON p.Id = po.ProductId
    JOIN Orders o ON po.OrderId = o.Id
    WHERE o.OdUser = @UserId
    GROUP BY m.MnName;
END;

-- Query 3: Find products in the same categories as a specified product
CREATE OR ALTER PROCEDURE GetProductsInSameCategories
    @ProductId INT
AS
BEGIN
    WITH ProductCategories AS (
        SELECT CategoryId
        FROM ProductCategories
        WHERE ProductId = @ProductId
    )
    SELECT p.PdName,
           STRING_AGG(c.CgName, ', ') as Categories
    FROM Products p
    JOIN ProductCategories pc ON p.Id = pc.ProductId
    JOIN Categories c ON pc.CategoryId = c.Id
    WHERE p.Id != @ProductId
    AND pc.CategoryId IN (SELECT CategoryId FROM ProductCategories)
    GROUP BY p.PdName;
END;

-- Query 4: Find users who haven't ordered any products that a specific user has ordered
CREATE OR ALTER PROCEDURE GetUsersWithDifferentOrders
    @UserId NVARCHAR(450)
AS
BEGIN
    WITH UserOrderedProducts AS (
        SELECT DISTINCT po.ProductId
        FROM Orders o
        JOIN ProductOrders po ON o.Id = po.OrderId
        WHERE o.OdUser = @UserId
    )
    SELECT u.UserName,
           COUNT(DISTINCT o.Id) as OrderCount
    FROM Users u
    JOIN Orders o ON u.Id = o.OdUser
    WHERE u.Id != @UserId
    AND NOT EXISTS (
        SELECT 1
        FROM ProductOrders po
        WHERE po.OrderId = o.Id
        AND po.ProductId IN (SELECT ProductId FROM UserOrderedProducts)
    )
    GROUP BY u.UserName;
END;

-- Query 5: Find products with price higher than average in their category
CREATE OR ALTER PROCEDURE GetProductsAboveCategoryAverage
AS
BEGIN
    WITH CategoryAverages AS (
        SELECT c.Id as CategoryId,
               c.CgName as Category,
               AVG(p.PdPrice) as AveragePrice
        FROM Categories c
        JOIN ProductCategories pc ON c.Id = pc.CategoryId
        JOIN Products p ON pc.ProductId = p.Id
        GROUP BY c.Id, c.CgName
    )
    SELECT p.PdName,
           p.PdPrice,
           ca.Category,
           ca.AveragePrice
    FROM Products p
    JOIN ProductCategories pc ON p.Id = pc.ProductId
    JOIN CategoryAverages ca ON pc.CategoryId = ca.CategoryId
    WHERE p.PdPrice > ca.AveragePrice;
END;

-- Query 6: Find manufacturers that supply all products in a specific category
CREATE OR ALTER PROCEDURE GetManufacturersWithAllCategoryProducts
    @CategoryId INT
AS
BEGIN
    WITH CategoryProducts AS (
        SELECT p.Id as ProductId
        FROM Products p
        JOIN ProductCategories pc ON p.Id = pc.ProductId
        WHERE pc.CategoryId = @CategoryId
    ),
    ManufacturerProducts AS (
        SELECT m.Id as ManufacturerId,
               m.MnName,
               COUNT(DISTINCT p.Id) as ProductCount
        FROM Manufacturers m
        JOIN Products p ON m.Id = p.ManufacturerId
        JOIN ProductCategories pc ON p.Id = pc.ProductId
        WHERE pc.CategoryId = @CategoryId
        GROUP BY m.Id, m.MnName
    )
    SELECT mp.MnName,
           mp.ProductCount
    FROM ManufacturerProducts mp
    WHERE mp.ProductCount = (SELECT COUNT(*) FROM CategoryProducts);
END;

-- Query 7: Find pairs of users who ordered exactly the same products
CREATE OR ALTER PROCEDURE GetUsersWithSameOrders
AS
BEGIN
    WITH UserOrderedProducts AS (
        SELECT o.OdUser,
               STRING_AGG(CAST(po.ProductId as NVARCHAR(10)), ',') WITHIN GROUP (ORDER BY po.ProductId) as ProductList
        FROM Orders o
        JOIN ProductOrders po ON o.Id = po.OrderId
        GROUP BY o.OdUser
    )
    SELECT u1.UserName as User1Name,
           u2.UserName as User2Name,
           STRING_AGG(p.PdName, ', ') as CommonProducts
    FROM UserOrderedProducts up1
    JOIN UserOrderedProducts up2 ON up1.ProductList = up2.ProductList AND up1.OdUser < up2.OdUser
    JOIN Users u1 ON up1.OdUser = u1.Id
    JOIN Users u2 ON up2.OdUser = u2.Id
    JOIN ProductOrders po ON po.OrderId IN (
        SELECT o.Id FROM Orders o WHERE o.OdUser = up1.OdUser
    )
    JOIN Products p ON po.ProductId = p.Id
    GROUP BY u1.UserName, u2.UserName;
END; 