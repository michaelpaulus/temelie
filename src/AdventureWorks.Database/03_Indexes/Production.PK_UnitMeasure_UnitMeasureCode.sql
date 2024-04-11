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
            indexes.name = 'PK_UnitMeasure_UnitMeasureCode' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[UnitMeasure] ADD CONSTRAINT [PK_UnitMeasure_UnitMeasureCode] PRIMARY KEY CLUSTERED
    (
    [UnitMeasureCode]
    )
GO
