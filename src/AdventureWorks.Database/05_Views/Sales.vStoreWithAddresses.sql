CREATE VIEW [Sales].[vStoreWithAddresses] AS
SELECT
    s.[BusinessEntityID],
    s.[Name],
    at.[Name] AS [AddressType],
    a.[AddressLine1],
    a.[AddressLine2],
    a.[City],
    sp.[Name] AS [StateProvinceName],
    a.[PostalCode],
    cr.[Name] AS [CountryRegionName]
FROM  
    [Sales].[Store] AS s INNER JOIN
    [Person].[BusinessEntityAddress] AS bea ON
         bea.[BusinessEntityID] = s.[BusinessEntityID] INNER JOIN
    [Person].[Address] AS a ON
         a.[AddressID] = bea.[AddressID] INNER JOIN
    [Person].[StateProvince] AS sp ON
         sp.[StateProvinceID] = a.[StateProvinceID] INNER JOIN
    [Person].[CountryRegion] AS cr ON
         cr.[CountryRegionCode] = sp.[CountryRegionCode] INNER JOIN
    [Person].[AddressType] AS at ON
         at.[AddressTypeID] = bea.[AddressTypeID];
GO