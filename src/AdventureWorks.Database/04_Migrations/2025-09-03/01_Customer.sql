IF EXISTS
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
        schemas.name = 'Sales' AND
		tables.name = 'Customer' AND
		columns.name = 'AccountNumber'
)
ALTER TABLE [Sales].[Customer] DROP COLUMN [AccountNumber]
GO
