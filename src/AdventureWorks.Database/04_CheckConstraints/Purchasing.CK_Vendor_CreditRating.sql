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
        check_constraints.name = 'CK_Vendor_CreditRating' AND
        schemas.name = 'Purchasing'
)
ALTER TABLE [Purchasing].[Vendor] WITH CHECK ADD CONSTRAINT [CK_Vendor_CreditRating] CHECK (([CreditRating]>=(1) AND [CreditRating]<=(5)))
GO
