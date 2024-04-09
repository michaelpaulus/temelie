-- vStoreWithContacts
DROP VIEW IF EXISTS [Sales].[vStoreWithContacts]
GO

CREATE VIEW [Sales].[vStoreWithContacts] AS 
SELECT 
    s.[BusinessEntityID] 
    ,s.[Name] 
    ,ct.[Name] AS [ContactType] 
    ,p.[Title] 
    ,p.[FirstName] 
    ,p.[MiddleName] 
    ,p.[LastName] 
    ,p.[Suffix] 
    ,pp.[PhoneNumber] 
    ,pnt.[Name] AS [PhoneNumberType]
    ,ea.[EmailAddress] 
    ,p.[EmailPromotion] 
FROM [Sales].[Store] s
    INNER JOIN [Person].[BusinessEntityContact] bec 
    ON bec.[BusinessEntityID] = s.[BusinessEntityID]
    INNER JOIN [Person].[ContactType] ct
    ON ct.[ContactTypeID] = bec.[ContactTypeID]
    INNER JOIN [Person].[Person] p
    ON p.[BusinessEntityID] = bec.[PersonID]
    LEFT OUTER JOIN [Person].[EmailAddress] ea
    ON ea.[BusinessEntityID] = p.[BusinessEntityID]
    LEFT OUTER JOIN [Person].[PersonPhone] pp
    ON pp.[BusinessEntityID] = p.[BusinessEntityID]
    LEFT OUTER JOIN [Person].[PhoneNumberType] pnt
    ON pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID];
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'view', 'vStoreWithContacts', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'view',
                                     @level1name = 'vStoreWithContacts';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Stores (including store contacts) that sell Adventure Works Cycles products to consumers.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'view',
                                @level1name = 'vStoreWithContacts';
GO

