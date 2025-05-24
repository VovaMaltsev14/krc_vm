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