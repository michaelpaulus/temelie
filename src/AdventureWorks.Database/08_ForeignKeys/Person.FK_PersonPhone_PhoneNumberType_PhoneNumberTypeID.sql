
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_PersonPhone_PhoneNumberType_PhoneNumberTypeID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[PersonPhone] WITH CHECK ADD CONSTRAINT [FK_PersonPhone_PhoneNumberType_PhoneNumberTypeID] FOREIGN KEY ([PhoneNumberTypeID])
    REFERENCES [Person].[PhoneNumberType] ([PhoneNumberTypeID])
GO
