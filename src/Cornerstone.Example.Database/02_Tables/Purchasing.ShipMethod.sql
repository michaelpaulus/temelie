-- ShipMethod

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ShipMethod' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[ShipMethod]
    (
        [ShipMethodID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ShipBase] MONEY NOT NULL DEFAULT (0.00),
        [ShipRate] MONEY NOT NULL DEFAULT (0.00),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping company lookup table.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', 'column', 'ShipMethodID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod',
                                     @level2type = N'column',
                                     @level2name = 'ShipMethodID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for ShipMethod records.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod',
                                @level2type = N'column',
                                @level2name = 'ShipMethodID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping company name.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', 'column', 'ShipBase')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod',
                                     @level2type = N'column',
                                     @level2name = 'ShipBase';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Minimum shipping charge.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod',
                                @level2type = N'column',
                                @level2name = 'ShipBase';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', 'column', 'ShipRate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod',
                                     @level2type = N'column',
                                     @level2name = 'ShipRate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping charge per pound.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod',
                                @level2type = N'column',
                                @level2name = 'ShipRate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ShipMethod', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ShipMethod',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ShipMethod',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

