-- JobCandidate

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'JobCandidate' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[JobCandidate]
    (
        [JobCandidateID] INT IDENTITY (1, 1) NOT NULL,
        [BusinessEntityID] INT NULL,
        [Resume] XML NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'JobCandidate', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'JobCandidate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Résumés submitted to Human Resources by job applicants.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'JobCandidate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'JobCandidate', 'column', 'JobCandidateID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'JobCandidate',
                                     @level2type = N'column',
                                     @level2name = 'JobCandidateID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for JobCandidate records.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'JobCandidate',
                                @level2type = N'column',
                                @level2name = 'JobCandidateID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'JobCandidate', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'JobCandidate',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee identification number if applicant was hired. Foreign key to Employee.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'JobCandidate',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'JobCandidate', 'column', 'Resume')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'JobCandidate',
                                     @level2type = N'column',
                                     @level2name = 'Resume';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Résumé in XML format.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'JobCandidate',
                                @level2type = N'column',
                                @level2name = 'Resume';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'JobCandidate', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'JobCandidate',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'JobCandidate',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

