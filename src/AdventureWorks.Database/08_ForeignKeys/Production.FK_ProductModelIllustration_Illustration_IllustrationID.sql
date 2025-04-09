﻿
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ProductModelIllustration_Illustration_IllustrationID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductModelIllustration] WITH CHECK ADD CONSTRAINT [FK_ProductModelIllustration_Illustration_IllustrationID] FOREIGN KEY ([IllustrationID])
    REFERENCES [Production].[Illustration] ([IllustrationID])
GO
