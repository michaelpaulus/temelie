-- vSalesPerson
DROP VIEW IF EXISTS [Sales].[vSalesPerson]
GO

CREATE VIEW [Sales].[vSalesPerson] AS
SELECT
    s.[BusinessEntityID],
    p.[Title],
    p.[FirstName],
    p.[MiddleName],
    p.[LastName],
    p.[Suffix],
    e.[JobTitle],
    pp.[PhoneNumber],
    pnt.[Name] AS [PhoneNumberType],
    ea.[EmailAddress],
    p.[EmailPromotion],
    a.[AddressLine1],
    a.[AddressLine2],
    a.[City],
    sp.[Name] AS [StateProvinceName],
    a.[PostalCode],
    cr.[Name] AS [CountryRegionName],
    st.[Name] AS [TerritoryName],
    st.[Group] AS [TerritoryGroup],
    s.[SalesQuota],
    s.[SalesYTD],
    s.[SalesLastYear]
FROM  
    [Sales].[SalesPerson] AS s INNER JOIN
    [HumanResources].[Employee] AS e ON
         e.[BusinessEntityID] = s.[BusinessEntityID] INNER JOIN
    [Person].[Person] AS p ON
         p.[BusinessEntityID] = s.[BusinessEntityID] INNER JOIN
    [Person].[BusinessEntityAddress] AS bea ON
         bea.[BusinessEntityID] = s.[BusinessEntityID] INNER JOIN
    [Person].[Address] AS a ON
         a.[AddressID] = bea.[AddressID] INNER JOIN
    [Person].[StateProvince] AS sp ON
         sp.[StateProvinceID] = a.[StateProvinceID] INNER JOIN
    [Person].[CountryRegion] AS cr ON
         cr.[CountryRegionCode] = sp.[CountryRegionCode] LEFT OUTER JOIN
    [Sales].[SalesTerritory] AS st ON
         st.[TerritoryID] = s.[TerritoryID] LEFT OUTER JOIN
    [Person].[EmailAddress] AS ea ON
         ea.[BusinessEntityID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[PersonPhone] AS pp ON
         pp.[BusinessEntityID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[PhoneNumberType] AS pnt ON
         pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID];
GO
