IF NOT EXISTS
(
	SELECT
		1
	FROM
		sys.schemas INNER JOIN
		sys.tables ON
			schemas.schema_id = tables.schema_id INNER JOIN
		sys.columns ON
			tables.object_id = columns.object_id
	WHERE
        schemas.name = 'Person' AND
		tables.name = 'Address' AND
		columns.name = 'CreatedBy'
)
ALTER TABLE [Person].[Address] ADD [CreatedBy] NVARCHAR(250) NULL
GO

UPDATE
	Person.Address
SET
	[CreatedBy] = ''
WHERE
	CreatedBy IS NULL
GO

IF NOT EXISTS
(
	SELECT
		1
	FROM
		sys.schemas INNER JOIN
		sys.tables ON
			schemas.schema_id = tables.schema_id INNER JOIN
		sys.columns ON
			tables.object_id = columns.object_id
	WHERE
        schemas.name = 'Person' AND
		tables.name = 'Address' AND
		columns.name = 'CreatedBy' AND
		columns.is_nullable = 1
)
ALTER TABLE Person.Address ALTER COLUMN CreatedBy NVARCHAR(250) NOT NULL
GO


IF NOT EXISTS
(
	SELECT
		1
	FROM
		sys.schemas INNER JOIN
		sys.tables ON
			schemas.schema_id = tables.schema_id INNER JOIN
		sys.columns ON
			tables.object_id = columns.object_id
	WHERE
        schemas.name = 'Person' AND
		tables.name = 'Address' AND
		columns.name = 'ModifiedBy'
)
ALTER TABLE [Person].[Address] ADD [ModifiedBy] NVARCHAR(250) NULL
GO

UPDATE
	Person.Address
SET
	[ModifiedBy] = ''
WHERE
	ModifiedBy IS NULL
GO

IF NOT EXISTS
(
	SELECT
		1
	FROM
		sys.schemas INNER JOIN
		sys.tables ON
			schemas.schema_id = tables.schema_id INNER JOIN
		sys.columns ON
			tables.object_id = columns.object_id
	WHERE
        schemas.name = 'Person' AND
		tables.name = 'Address' AND
		columns.name = 'ModifiedBy' AND
		columns.is_nullable = 1
)
ALTER TABLE Person.Address ALTER COLUMN ModifiedBy NVARCHAR(250) NOT NULL
GO
