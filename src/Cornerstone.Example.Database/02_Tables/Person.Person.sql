-- Person

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Person' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[Person]
    (
        [BusinessEntityID] INT NOT NULL,
        [PersonType] NCHAR(2) NOT NULL,
        [NameStyle] NAMESTYLE NOT NULL DEFAULT (0),
        [Title] NVARCHAR(8) NULL,
        [FirstName] NAME NOT NULL,
        [MiddleName] NAME NULL,
        [LastName] NAME NOT NULL,
        [Suffix] NVARCHAR(10) NULL,
        [EmailPromotion] INT NOT NULL DEFAULT (0),
        [AdditionalContactInfo] XML NULL,
        [Demographics] XML NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Person records.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'PersonType')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'PersonType';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary type of person: SC = Store Contact, IN = Individual (retail) customer, SP = Sales person, EM = Employee (non-sales), VC = Vendor contact, GC = General contact',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'PersonType';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'NameStyle')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'NameStyle';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'NameStyle';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'Title')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'Title';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'A courtesy title. For example, Mr. or Ms.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'Title';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'FirstName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'FirstName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'First name of the person.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'FirstName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'MiddleName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'MiddleName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Middle name or middle initial of the person.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'MiddleName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'LastName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'LastName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Last name of the person.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'LastName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'Suffix')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'Suffix';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Surname suffix. For example, Sr. or Jr.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'Suffix';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'EmailPromotion')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'EmailPromotion';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Contact does not wish to receive e-mail promotions, 1 = Contact does wish to receive e-mail promotions from AdventureWorks, 2 = Contact does wish to receive e-mail promotions from AdventureWorks and selected partners. ',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'EmailPromotion';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'AdditionalContactInfo')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'AdditionalContactInfo';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Additional contact information about the person stored in xml format. ',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'AdditionalContactInfo';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'Demographics')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'Demographics';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Personal information such as hobbies, and income collected from online shoppers. Used for sales analysis.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'Demographics';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Person', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Person',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Person',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

