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
            indexes.name = 'PK_Shift_ShiftID' AND
            schemas.name = 'HumanResources'
    )
    ALTER TABLE [HumanResources].[Shift] ADD CONSTRAINT [PK_Shift_ShiftID] PRIMARY KEY CLUSTERED
    (
    [ShiftID]
    )
GO
