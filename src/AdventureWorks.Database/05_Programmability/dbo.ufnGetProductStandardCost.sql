﻿CREATE FUNCTION [dbo].[ufnGetProductStandardCost]
(@ProductID INT, @OrderDate DATETIME)
RETURNS MONEY
AS
BEGIN
    DECLARE @StandardCost AS MONEY;
    SELECT
        @StandardCost = pch.[StandardCost]
    FROM  
        [Production].[Product] AS p INNER JOIN
        [Production].[ProductCostHistory] AS pch ON
             p.[ProductID] = pch.[ProductID] AND p.[ProductID] = @ProductID AND @OrderDate BETWEEN pch.[StartDate] AND COALESCE (pch.[EndDate], CONVERT (DATETIME, '99991231', 112));
    RETURN @StandardCost;
END
GO