﻿IF EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_Vendor_BusinessEntity_BusinessEntityID' AND
            schemas.name = 'Purchasing'
    )
BEGIN
    IF NOT EXISTS
        (
            SELECT
                1
            FROM
                sys.foreign_keys INNER JOIN
                sys.schemas ON
                    foreign_keys.schema_id = schemas.schema_id
            WHERE
                foreign_keys.name = 'FK_Vendor_BusinessEntity_BusinessEntityID' AND
                schemas.name = 'Purchasing' AND
                foreign_keys.delete_referential_action_desc = 'NO_ACTION' AND
                foreign_keys.update_referential_action_desc = 'NO_ACTION' AND
                EXISTS
                (
                    SELECT
                        1
                    FROM
                        sys.foreign_key_columns INNER JOIN
                        sys.tables ON
                            foreign_key_columns.parent_object_id = tables.object_id INNER JOIN
                        sys.columns ON
                            tables.object_id = columns.object_id AND
                            columns.column_id = foreign_key_columns.parent_column_id INNER JOIN
                        sys.schemas ON
                            tables.schema_id = schemas.schema_id
                    WHERE
                        foreign_key_columns.constraint_object_id = foreign_keys.object_id AND
                        schemas.name = 'Purchasing' AND
                        tables.name = 'Vendor' AND
                        columns.name = 'BusinessEntityID' AND
                        foreign_key_columns.constraint_column_id = 1
                ) AND
                EXISTS
                (
                    SELECT
                        1
                    FROM
                        sys.foreign_key_columns INNER JOIN
                        sys.tables ON
                            foreign_key_columns.referenced_object_id = tables.object_id INNER JOIN
                        sys.columns ON
                            tables.object_id = columns.object_id AND
                            columns.column_id = foreign_key_columns.referenced_column_id INNER JOIN
                        sys.schemas ON
                            tables.schema_id = schemas.schema_id
                    WHERE
                        foreign_key_columns.constraint_object_id = foreign_keys.object_id AND
                        schemas.name = 'Person' AND
                        tables.name = 'BusinessEntity' AND
                        columns.name = 'BusinessEntityID' AND
                        foreign_key_columns.constraint_column_id = 1
                )
        )
    BEGIN
        ALTER TABLE [Purchasing].[Vendor] DROP CONSTRAINT [FK_Vendor_BusinessEntity_BusinessEntityID]
    END
END
GO

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_Vendor_BusinessEntity_BusinessEntityID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[Vendor] WITH CHECK ADD CONSTRAINT [FK_Vendor_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID])
    REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO