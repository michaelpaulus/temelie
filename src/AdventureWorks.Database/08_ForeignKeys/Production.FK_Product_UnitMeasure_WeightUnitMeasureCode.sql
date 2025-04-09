
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_Product_UnitMeasure_WeightUnitMeasureCode' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[Product] WITH CHECK ADD CONSTRAINT [FK_Product_UnitMeasure_WeightUnitMeasureCode] FOREIGN KEY ([WeightUnitMeasureCode])
    REFERENCES [Production].[UnitMeasure] ([UnitMeasureCode])
GO
