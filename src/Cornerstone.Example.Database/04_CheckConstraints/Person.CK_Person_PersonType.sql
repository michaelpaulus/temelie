
IF EXISTS
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
        check_constraints.name = 'CK_Person_PersonType' AND
        schemas.name = 'Person'
)
BEGIN
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
            check_constraints.name = 'CK_Person_PersonType' AND
            schemas.name = 'Person' AND
            check_constraints.definition = '([PersonType] IS NULL OR (upper([PersonType])=''GC'' OR upper([PersonType])=''SP'' OR upper([PersonType])=''EM'' OR upper([PersonType])=''IN'' OR upper([PersonType])=''VC'' OR upper([PersonType])=''SC''))'
    )
BEGIN
ALTER TABLE [Person].[Person] DROP CONSTRAINT [CK_Person_PersonType]
END
END
GO

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
        check_constraints.name = 'CK_Person_PersonType' AND
        schemas.name = 'Person'
)
ALTER TABLE [Person].[Person] WITH CHECK ADD CONSTRAINT [CK_Person_PersonType] CHECK (([PersonType] IS NULL OR (upper([PersonType])='GC' OR upper([PersonType])='SP' OR upper([PersonType])='EM' OR upper([PersonType])='IN' OR upper([PersonType])='VC' OR upper([PersonType])='SC')))
GO
