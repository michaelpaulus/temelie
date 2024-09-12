-- ufnGetProductDealerPrice
DROP FUNCTION IF EXISTS [dbo].[ufnGetProductDealerPrice]
GO

CREATE FUNCTION [dbo].[ufnGetProductDealerPrice]
(@ProductID INT, @OrderDate DATETIME)
RETURNS MONEY
AS
BEGIN
    DECLARE @DealerPrice AS MONEY;
    DECLARE @DealerDiscount AS MONEY;
    SET @DealerDiscount = 0.60;
    SELECT
        @DealerPrice = plph.[ListPrice] * @DealerDiscount
    FROM  
        [Production].[Product] AS p INNER JOIN
        [Production].[ProductListPriceHistory] AS plph ON
             p.[ProductID] = plph.[ProductID] AND p.[ProductID] = @ProductID AND @OrderDate BETWEEN plph.[StartDate] AND COALESCE (plph.[EndDate], CONVERT (DATETIME, '99991231', 112));
    RETURN @DealerPrice;
END
GO
