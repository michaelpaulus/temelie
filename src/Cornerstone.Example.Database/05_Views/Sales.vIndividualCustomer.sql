-- vIndividualCustomer
DROP VIEW IF EXISTS [Sales].[vIndividualCustomer]
GO

CREATE VIEW [Sales].[vIndividualCustomer] 
AS 
SELECT 
    p.[BusinessEntityID]
    ,p.[Title]
    ,p.[FirstName]
    ,p.[MiddleName]
    ,p.[LastName]
    ,p.[Suffix]
    ,pp.[PhoneNumber]
    ,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress]
    ,p.[EmailPromotion]
    ,at.[Name] AS [AddressType]
    ,a.[AddressLine1]
    ,a.[AddressLine2]
    ,a.[City]
    ,[StateProvinceName] = sp.[Name]
    ,a.[PostalCode]
    ,[CountryRegionName] = cr.[Name]
    ,p.[Demographics]
FROM [Person].[Person] p
    INNER JOIN [Person].[BusinessEntityAddress] bea 
    ON bea.[BusinessEntityID] = p.[BusinessEntityID] 
    INNER JOIN [Person].[Address] a 
    ON a.[AddressID] = bea.[AddressID]
    INNER JOIN [Person].[StateProvince] sp 
    ON sp.[StateProvinceID] = a.[StateProvinceID]
    INNER JOIN [Person].[CountryRegion] cr 
    ON cr.[CountryRegionCode] = sp.[CountryRegionCode]
    INNER JOIN [Person].[AddressType] at 
    ON at.[AddressTypeID] = bea.[AddressTypeID]
    INNER JOIN [Sales].[Customer] c
    ON c.[PersonID] = p.[BusinessEntityID]
    LEFT OUTER JOIN [Person].[EmailAddress] ea
    ON ea.[BusinessEntityID] = p.[BusinessEntityID]
    LEFT OUTER JOIN [Person].[PersonPhone] pp
    ON pp.[BusinessEntityID] = p.[BusinessEntityID]
    LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
    ON pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID]
WHERE c.StoreID IS NULL;
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'view', 'vIndividualCustomer', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'view',
                                     @level1name = 'vIndividualCustomer';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Individual customers (names and addresses) that purchase Adventure Works Cycles products online.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'view',
                                @level1name = 'vIndividualCustomer';
GO

