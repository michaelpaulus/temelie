﻿
IF NOT EXISTS
(
    SELECT
        1
    FROM
        sys.check_constraints INNER JOIN
        sys.tables ON
            check_constraints.parent_object_id = tables.object_id INNER JOIN
        sys.schemas ON
            tables.schema_id = schemas.schema_id
    WHERE
        check_constraints.name = 'CK_SalesTerritory_SalesYTD' AND
        schemas.name = 'Sales'
)
ALTER TABLE [Sales].[SalesTerritory] WITH CHECK ADD CONSTRAINT [CK_SalesTerritory_SalesYTD] CHECK (([SalesYTD]>=(0.00)))
GO
