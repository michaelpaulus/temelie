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
