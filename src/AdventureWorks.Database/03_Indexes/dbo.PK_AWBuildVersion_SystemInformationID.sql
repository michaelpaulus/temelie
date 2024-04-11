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
            indexes.name = 'PK_AWBuildVersion_SystemInformationID' AND
            schemas.name = 'dbo'
    )
    ALTER TABLE [dbo].[AWBuildVersion] ADD CONSTRAINT [PK_AWBuildVersion_SystemInformationID] PRIMARY KEY CLUSTERED
    (
    [SystemInformationID]
    )
GO
