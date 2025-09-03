CREATE VIEW [Purchasing].[vVendorWithContacts] AS
SELECT
    v.[BusinessEntityID],
    v.[Name],
    ct.[Name] AS [ContactType],
    p.[Title],
    p.[FirstName],
    p.[MiddleName],
    p.[LastName],
    p.[Suffix],
    pp.[PhoneNumber],
    pnt.[Name] AS [PhoneNumberType],
    ea.[EmailAddress],
    p.[EmailPromotion]
FROM  
    [Purchasing].[Vendor] AS v INNER JOIN
    [Person].[BusinessEntityContact] AS bec ON
         bec.[BusinessEntityID] = v.[BusinessEntityID] INNER JOIN
    [Person].ContactType AS ct ON
         ct.[ContactTypeID] = bec.[ContactTypeID] INNER JOIN
    [Person].[Person] AS p ON
         p.[BusinessEntityID] = bec.[PersonID] LEFT OUTER JOIN
    [Person].[EmailAddress] AS ea ON
         ea.[BusinessEntityID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[PersonPhone] AS pp ON
         pp.[BusinessEntityID] = p.[BusinessEntityID] LEFT OUTER JOIN
    [Person].[PhoneNumberType] AS pnt ON
         pnt.[PhoneNumberTypeID] = pp.[PhoneNumberTypeID];
GO