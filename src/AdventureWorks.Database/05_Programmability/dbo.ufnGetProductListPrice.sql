﻿-- ufnGetProductListPrice
DROP FUNCTION IF EXISTS [dbo].[ufnGetProductListPrice]
GO

CREATE FUNCTION [dbo].[ufnGetProductListPrice]
(@ProductID INT, @OrderDate DATETIME)
RETURNS MONEY
AS
BEGIN
    DECLARE @ListPrice AS MONEY;
    SELECT
        @ListPrice = plph.[ListPrice]
    FROM  
        [Production].[Product] AS p INNER JOIN
        [Production].[ProductListPriceHistory] AS plph ON
             p.[ProductID] = plph.[ProductID] AND p.[ProductID] = @ProductID AND @OrderDate BETWEEN plph.[StartDate] AND COALESCE (plph.[EndDate], CONVERT (DATETIME, '99991231', 112));
    RETURN @ListPrice;
END
GO
