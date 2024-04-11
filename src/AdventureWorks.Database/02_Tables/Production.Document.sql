-- Document

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Document' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[Document]
    (
        [DocumentNode] HIERARCHYID NOT NULL,
        [DocumentLevel] AS ([DocumentNode].[GetLevel]()),
        [Title] NVARCHAR(50) NOT NULL,
        [Owner] INT NOT NULL,
        [FolderFlag] BIT NOT NULL DEFAULT (0),
        [FileName] NVARCHAR(400) NOT NULL,
        [FileExtension] NVARCHAR(8) NOT NULL,
        [Revision] NCHAR(5) NOT NULL,
        [ChangeNumber] INT NOT NULL DEFAULT (0),
        [Status] TINYINT NOT NULL,
        [DocumentSummary] NVARCHAR(MAX) NULL,
        [Document] VARBINARY(MAX) NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product maintenance documents.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'DocumentNode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'DocumentNode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Document records.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'DocumentNode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'DocumentLevel')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'DocumentLevel';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Depth in the document hierarchy.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'DocumentLevel';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'Title')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'Title';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Title of the document.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'Title';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'Owner')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'Owner';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee who controls the document.  Foreign key to Employee.BusinessEntityID',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'Owner';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'FolderFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'FolderFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = This is a folder, 1 = This is a document.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'FolderFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'FileName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'FileName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'File name of the document',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'FileName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'FileExtension')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'FileExtension';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'File extension indicating the document type. For example, .doc or .txt.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'FileExtension';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'Revision')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'Revision';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Revision number of the document. ',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'Revision';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'ChangeNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'ChangeNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Engineering change approval number.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'ChangeNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'Status')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'Status';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'1 = Pending approval, 2 = Approved, 3 = Obsolete',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'Status';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'DocumentSummary')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'DocumentSummary';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Document abstract.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'DocumentSummary';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'Document')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'Document';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Complete document.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'Document';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Required for FileStream.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Document', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Document',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Document',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

