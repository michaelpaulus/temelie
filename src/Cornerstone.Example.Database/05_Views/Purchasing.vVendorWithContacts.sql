-- vVendorWithContacts
DROP VIEW IF EXISTS [Purchasing].[vVendorWithContacts]
GO

CREATE VIEW [Purchasing].[vVendorWithContacts] AS 
SELECT 
    v.[BusinessEntityID]
    ,v.[Name]
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
FROM [Purchasing].[Vendor] v
    INNER JOIN [Person].[BusinessEntityContact] bec 
    ON bec.[BusinessEntityID] = v.[BusinessEntityID]
    INNER JOIN [Person].ContactType ct
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
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'view', 'vVendorWithContacts', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'view',
                                     @level1name = 'vVendorWithContacts';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Vendor (company) names  and the names of vendor employees to contact.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'view',
                                @level1name = 'vVendorWithContacts';
GO

