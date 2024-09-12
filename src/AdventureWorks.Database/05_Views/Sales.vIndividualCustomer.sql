-- vIndividualCustomer
DROP VIEW IF EXISTS [Sales].[vIndividualCustomer]
GO

CREATE VIEW [Sales].[vIndividualCustomer] AS
SELECT
    p.[BusinessEntityID],
    p.[Title],
    p.[FirstName],
    p.[MiddleName],
    p.[LastName],
    p.[Suffix],
    pp.[PhoneNumber],
    pnt.[Name] AS [PhoneNumberType],
    ea.[EmailAddress],
    p.[EmailPromotion],
    at.[Name] AS [AddressType],
    a.[AddressLine1],
    a.[AddressLine2],
    a.[City],
    sp.[Name] AS [StateProvinceName],
    a.[PostalCode],
    cr.[Name] AS [CountryRegionName],
    p.[Demographics]
FROM  
    [Person].[Person] AS p INNER JOIN
    [Person].[BusinessEntityAddress] AS bea ON
         bea.[BusinessEntityID] = p.[BusinessEntityID] INNER JOIN
    [Person].[Address] AS a ON
         a.[AddressID] = bea.[AddressID] INNER JOIN
    [Person].[StateProvince] AS sp ON
         sp.[StateProvinceID] = a.[StateProvinceID] INNER JOIN
    [Person].[CountryRegion] AS cr ON
         cr.[CountryRegionCode] = sp.[CountryRegionCode] INNER JOIN
    [Person].[AddressType] AS at ON
         at.[AddressTypeID] = bea.[AddressTypeID] INNER JOIN
    [Sales].[Customer] AS c ON
         c.[PersonID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[EmailAddress] AS ea ON
         ea.[BusinessEntityID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[PersonPhone] AS pp ON
         pp.[BusinessEntityID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[PhoneNumberType] AS pnt ON
         pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID] WHERE
                                                               c.StoreID IS NULL;
GO
