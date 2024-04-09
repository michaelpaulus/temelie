-- vEmployeeDepartment
DROP VIEW IF EXISTS [HumanResources].[vEmployeeDepartment]
GO

CREATE VIEW [HumanResources].[vEmployeeDepartment] 
AS 
SELECT 
    e.[BusinessEntityID] 
    ,p.[Title] 
    ,p.[FirstName] 
    ,p.[MiddleName] 
    ,p.[LastName] 
    ,p.[Suffix] 
    ,e.[JobTitle]
    ,d.[Name] AS [Department] 
    ,d.[GroupName] 
    ,edh.[StartDate] 
FROM [HumanResources].[Employee] e
    INNER JOIN [Person].[Person] p
    ON p.[BusinessEntityID] = e.[BusinessEntityID]
    INNER JOIN [HumanResources].[EmployeeDepartmentHistory] edh 
    ON e.[BusinessEntityID] = edh.[BusinessEntityID] 
    INNER JOIN [HumanResources].[Department] d 
    ON edh.[DepartmentID] = d.[DepartmentID] 
WHERE edh.EndDate IS NULL
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'view', 'vEmployeeDepartment', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'view',
                                     @level1name = 'vEmployeeDepartment';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Returns employee name, title, and current department.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'view',
                                @level1name = 'vEmployeeDepartment';
GO

