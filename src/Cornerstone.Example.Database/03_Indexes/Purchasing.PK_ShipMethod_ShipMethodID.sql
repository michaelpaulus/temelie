IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = 'PK_ShipMethod_ShipMethodID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[ShipMethod] ADD CONSTRAINT [PK_ShipMethod_ShipMethodID] PRIMARY KEY CLUSTERED
    (
    [ShipMethodID]
    )
GO
