-- Stored procedure to find products that haven't been ordered and have quantity less than specified
CREATE PROCEDURE GetUnorderedProductsByQuantity
    @MaxQuantity INT
AS
BEGIN
    SELECT p.pd_name, p.pd_quantity, p.pd_price, m.mn_name
    FROM products p
    JOIN manufacturers m ON p.manufacturer_id = m.mn_id
    WHERE p.pd_id NOT IN (SELECT product_id FROM product_orders)
    AND p.pd_quantity < @MaxQuantity;
END;
GO

-- Stored procedure to find manufacturers that supply products ordered by a specific user
CREATE PROCEDURE GetManufacturersByUserOrders
    @UserId NVARCHAR(450)
AS
BEGIN
    SELECT DISTINCT m.mn_name, STRING_AGG(p.pd_name, ', ') as Products
    FROM manufacturers m
    JOIN products p ON m.mn_id = p.manufacturer_id
    JOIN product_orders po ON p.pd_id = po.product_id
    JOIN orders o ON po.order_id = o.od_id
    WHERE o.od_user = @UserId
    GROUP BY m.mn_id, m.mn_name;
END;
GO

-- Stored procedure to find products in the same categories as a specified product
CREATE PROCEDURE GetProductsInSameCategories
    @ProductId INT
AS
BEGIN
    SELECT DISTINCT p2.pd_name, STRING_AGG(c.cg_name, ', ') as Categories
    FROM products p1
    JOIN product_categories pc1 ON p1.pd_id = pc1.product_id
    JOIN product_categories pc2 ON pc1.category_id = pc2.category_id
    JOIN products p2 ON pc2.product_id = p2.pd_id
    JOIN categories c ON pc2.category_id = c.cg_id
    WHERE p1.pd_id = @ProductId AND p2.pd_id != @ProductId
    GROUP BY p2.pd_id, p2.pd_name;
END;
GO

-- Stored procedure to find users who haven't ordered any products that a specific user has ordered
CREATE PROCEDURE GetUsersWithDifferentOrders
    @UserId NVARCHAR(450)
AS
BEGIN
    SELECT u.UserName as UsName, COUNT(DISTINCT o.od_id) as OrderCount
    FROM AspNetUsers u
    JOIN orders o ON u.Id = o.od_user
    WHERE u.Id != @UserId
    AND NOT EXISTS (
        SELECT 1
        FROM orders o1
        JOIN product_orders po1 ON o1.od_id = po1.order_id
        WHERE o1.od_user = @UserId
        AND EXISTS (
            SELECT 1
            FROM product_orders po2
            WHERE po2.order_id = o.od_id
            AND po2.product_id = po1.product_id
        )
    )
    GROUP BY u.Id, u.UserName;
END;
GO

-- Stored procedure to find products with price higher than average in their category
CREATE PROCEDURE GetProductsAboveCategoryAverage
AS
BEGIN
    WITH CategoryAverages AS (
        SELECT 
            c.cg_id,
            c.cg_name,
            AVG(CAST(p.pd_price AS DECIMAL(18,2))) as AvgPrice
        FROM categories c
        JOIN product_categories pc ON c.cg_id = pc.category_id
        JOIN products p ON pc.product_id = p.pd_id
        GROUP BY c.cg_id, c.cg_name
    )
    SELECT 
        p.pd_name,
        p.pd_price,
        ca.cg_name,
        ca.AvgPrice
    FROM products p
    JOIN product_categories pc ON p.pd_id = pc.product_id
    JOIN CategoryAverages ca ON pc.category_id = ca.cg_id
    WHERE CAST(p.pd_price AS DECIMAL(18,2)) > ca.AvgPrice;
END;
GO

-- Stored procedure to find manufacturers that supply all products in a specific category
CREATE PROCEDURE GetManufacturersWithAllCategoryProducts
    @CategoryId INT
AS
BEGIN
    WITH CategoryProducts AS (
        SELECT COUNT(DISTINCT product_id) as TotalProducts
        FROM product_categories
        WHERE category_id = @CategoryId
    )
    SELECT 
        m.mn_name,
        m.mn_contact_info as MnCountry,
        COUNT(DISTINCT p.pd_id) as ProductCount
    FROM manufacturers m
    JOIN products p ON m.mn_id = p.manufacturer_id
    JOIN product_categories pc ON p.pd_id = pc.product_id
    WHERE pc.category_id = @CategoryId
    GROUP BY m.mn_id, m.mn_name, m.mn_contact_info
    HAVING COUNT(DISTINCT p.pd_id) = (SELECT TotalProducts FROM CategoryProducts);
END;
GO

-- Stored procedure to find users who ordered exactly the same products
CREATE PROCEDURE GetUsersWithSameOrders
AS
BEGIN
    WITH UserProducts AS (
        SELECT 
            o.od_user,
            u.UserName,
            p.pd_id,
            p.pd_name
        FROM orders o
        JOIN AspNetUsers u ON o.od_user = u.Id
        JOIN product_orders po ON o.od_id = po.order_id
        JOIN products p ON po.product_id = p.pd_id
    ),
    CommonProducts AS (
        SELECT 
            up1.UserName as User1,
            up2.UserName as User2,
            STRING_AGG(up1.pd_name, ', ') WITHIN GROUP (ORDER BY up1.pd_name) as CommonProducts,
            COUNT(DISTINCT up1.pd_id) as CommonProductCount
        FROM UserProducts up1
        JOIN UserProducts up2 ON up1.pd_id = up2.pd_id AND up1.od_user < up2.od_user
        GROUP BY up1.UserName, up2.UserName
        HAVING COUNT(DISTINCT up1.pd_id) > 0
    )
    SELECT 
        User1,
        User2,
        CommonProducts,
        CommonProductCount
    FROM CommonProducts
    ORDER BY CommonProductCount DESC, User1, User2;
END;
GO

-- Stored procedure to find products that share ALL categories with a specified product
CREATE PROCEDURE GetProductsWithAllSameCategories
    @ProductId INT
AS
BEGIN
    WITH ProductCategories AS (
        SELECT category_id
        FROM product_categories
        WHERE product_id = @ProductId
    )
    SELECT p.pd_name, STRING_AGG(c.cg_name, ', ') as Categories
    FROM products p
    JOIN product_categories pc ON p.pd_id = pc.product_id
    JOIN categories c ON pc.category_id = c.cg_id
    WHERE p.pd_id != @ProductId
    GROUP BY p.pd_id, p.pd_name
    HAVING COUNT(DISTINCT pc.category_id) = (SELECT COUNT(*) FROM ProductCategories)
    AND COUNT(DISTINCT pc.category_id) = (
        SELECT COUNT(DISTINCT pc2.category_id)
        FROM product_categories pc2
        WHERE pc2.product_id = p.pd_id
        AND pc2.category_id IN (SELECT category_id FROM ProductCategories)
    );
END;
GO

-- Stored procedure to find products ordered by all users in a specific shipping country
CREATE PROCEDURE GetProductsOrderedByAllUsersInCountry
    @CountryId INT
AS
BEGIN
    WITH UsersInCountry AS (
        SELECT DISTINCT o.od_user
        FROM orders o
        JOIN shipings s ON o.shipping_id = s.sh_id
        WHERE s.country_id = @CountryId
    ),
    UserOrderedProducts AS (
        SELECT DISTINCT o.od_user, po.product_id
        FROM orders o
        JOIN product_orders po ON o.od_id = po.order_id
        WHERE o.od_user IN (SELECT od_user FROM UsersInCountry)
    )
    SELECT 
        p.pd_name as ProductName,
        m.mn_name as ManufacturerName,
        COUNT(DISTINCT uop.od_user) as UserCount,
        (SELECT COUNT(*) FROM UsersInCountry) as TotalUsersInCountry
    FROM products p
    JOIN manufacturers m ON p.manufacturer_id = m.mn_id
    JOIN UserOrderedProducts uop ON p.pd_id = uop.product_id
    GROUP BY p.pd_id, p.pd_name, m.mn_name
    HAVING COUNT(DISTINCT uop.od_user) = (SELECT COUNT(*) FROM UsersInCountry);
END;
GO

-- Stored procedure to find manufacturers that supply products in all categories
CREATE PROCEDURE GetManufacturersWithAllCategories
AS
BEGIN
    WITH CategoryCount AS (
        SELECT COUNT(DISTINCT cg_id) as TotalCategories
        FROM categories
    ),
    ManufacturerCategoryCount AS (
        SELECT 
            m.mn_id,
            m.mn_name,
            COUNT(DISTINCT pc.category_id) as CategoryCount
        FROM manufacturers m
        JOIN products p ON m.mn_id = p.manufacturer_id
        JOIN product_categories pc ON p.pd_id = pc.product_id
        GROUP BY m.mn_id, m.mn_name
    )
    SELECT 
        mcc.mn_name as ManufacturerName,
        c.co_name as Country,
        mcc.CategoryCount as CategoriesSupplied,
        (SELECT TotalCategories FROM CategoryCount) as TotalCategories
    FROM ManufacturerCategoryCount mcc
    JOIN manufacturers m ON mcc.mn_id = m.mn_id
    JOIN countries c ON m.country_id = c.co_id
    WHERE mcc.CategoryCount = (SELECT TotalCategories FROM CategoryCount);
END;
GO

-- Stored procedure to find users who ordered products from all manufacturers
CREATE PROCEDURE GetUsersWithAllManufacturers
AS
BEGIN
    WITH ManufacturerCount AS (
        SELECT COUNT(DISTINCT mn_id) as TotalManufacturers
        FROM manufacturers
    ),
    UserManufacturerCount AS (
        SELECT 
            o.od_user,
            u.UserName,
            COUNT(DISTINCT p.manufacturer_id) as ManufacturerCount
        FROM orders o
        JOIN AspNetUsers u ON o.od_user = u.Id
        JOIN product_orders po ON o.od_id = po.order_id
        JOIN products p ON po.product_id = p.pd_id
        GROUP BY o.od_user, u.UserName
    )
    SELECT 
        umc.UserName,
        umc.ManufacturerCount as ManufacturersOrderedFrom,
        (SELECT TotalManufacturers FROM ManufacturerCount) as TotalManufacturers
    FROM UserManufacturerCount umc
    WHERE umc.ManufacturerCount = (SELECT TotalManufacturers FROM ManufacturerCount);
END;
GO 


CREATE OR ALTER PROCEDURE GetManufacturersWithSameCustomerBirthMonths
AS
BEGIN
    WITH ManufacturerCustomerMonths AS (
        SELECT m.mn_id, m.mn_name,
        STRING_AGG(CAST(MONTH(u.UrBirthdate) AS VARCHAR), ',') WITHIN GROUP (ORDER BY MONTH(u.UrBirthdate)) as BirthMonths
        FROM manufacturers m
        JOIN products p ON m.mn_id = p.manufacturer_id
        JOIN product_orders po ON p.pd_id = po.product_id
        JOIN orders o ON po.order_id = o.od_id
        JOIN AspNetUsers u ON o.od_user = u.Id
        GROUP BY m.mn_id, m.mn_name
    )
    SELECT 
        mcm1.mn_name as Manufacturer1, 
        mcm2.mn_name as Manufacturer2,
        mcm1.BirthMonths as CommonBirthMonths,
        LEN(mcm1.BirthMonths) - LEN(REPLACE(mcm1.BirthMonths, ',', '')) + 1 as MonthCount
    FROM ManufacturerCustomerMonths mcm1
    JOIN ManufacturerCustomerMonths mcm2 ON mcm1.BirthMonths = mcm2.BirthMonths AND mcm1.mn_id < mcm2.mn_id
    WHERE mcm1.BirthMonths IS NOT NULL AND mcm1.BirthMonths != '';
END 