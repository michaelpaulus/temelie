-- vSalesPersonSalesByFiscalYears
DROP VIEW IF EXISTS [Sales].[vSalesPersonSalesByFiscalYears]
GO

CREATE VIEW [Sales].[vSalesPersonSalesByFiscalYears] 
AS 
SELECT 
    pvt.[SalesPersonID]
    ,pvt.[FullName]
    ,pvt.[JobTitle]
    ,pvt.[SalesTerritory]
    ,pvt.[2002]
    ,pvt.[2003]
    ,pvt.[2004] 
FROM (SELECT 
        soh.[SalesPersonID]
        ,p.[FirstName] + ' ' + COALESCE(p.[MiddleName], '') + ' ' + p.[LastName] AS [FullName]
        ,e.[JobTitle]
        ,st.[Name] AS [SalesTerritory]
        ,soh.[SubTotal]
        ,YEAR(DATEADD(m, 6, soh.[OrderDate])) AS [FiscalYear] 
    FROM [Sales].[SalesPerson] sp 
        INNER JOIN [Sales].[SalesOrderHeader] soh 
        ON sp.[BusinessEntityID] = soh.[SalesPersonID]
        INNER JOIN [Sales].[SalesTerritory] st 
        ON sp.[TerritoryID] = st.[TerritoryID] 
        INNER JOIN [HumanResources].[Employee] e 
        ON soh.[SalesPersonID] = e.[BusinessEntityID] 
        INNER JOIN [Person].[Person] p
        ON p.[BusinessEntityID] = sp.[BusinessEntityID]
     ) AS soh 
PIVOT 
(
    SUM([SubTotal]) 
    FOR [FiscalYear] 
    IN ([2002], [2003], [2004])
) AS pvt;
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'view', 'vSalesPersonSalesByFiscalYears', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'view',
                                     @level1name = 'vSalesPersonSalesByFiscalYears';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Uses PIVOT to return aggregated sales information for each sales representative.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'view',
                                @level1name = 'vSalesPersonSalesByFiscalYears';
GO

