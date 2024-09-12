-- vSalesPersonSalesByFiscalYears
DROP VIEW IF EXISTS [Sales].[vSalesPersonSalesByFiscalYears]
GO

CREATE VIEW [Sales].[vSalesPersonSalesByFiscalYears] AS
SELECT
    pvt.[SalesPersonID],
    pvt.[FullName],
    pvt.[JobTitle],
    pvt.[SalesTerritory],
    pvt.[2002],
    pvt.[2003],
    pvt.[2004]
FROM  
    (SELECT
         soh.[SalesPersonID],
         p.[FirstName] + ' ' + COALESCE (p.[MiddleName], '') + ' ' + p.[LastName] AS [FullName],
         e.[JobTitle],
         st.[Name] AS [SalesTerritory],
         soh.[SubTotal],
         YEAR(DATEADD(m, 6, soh.[OrderDate])) AS [FiscalYear]
     FROM  
         [Sales].[SalesPerson] AS sp INNER JOIN
         [Sales].[SalesOrderHeader] AS soh ON
              sp.[BusinessEntityID] = soh.[SalesPersonID] INNER JOIN
         [Sales].[SalesTerritory] AS st ON
              sp.[TerritoryID] = st.[TerritoryID] INNER JOIN
         [HumanResources].[Employee] AS e ON
              soh.[SalesPersonID] = e.[BusinessEntityID] INNER JOIN
         [Person].[Person] AS p ON
              p.[BusinessEntityID] = sp.[BusinessEntityID]) AS soh PIVOT (SUM ([SubTotal]) FOR [FiscalYear] IN ([2002], [2003], [2004])) AS pvt;
GO
