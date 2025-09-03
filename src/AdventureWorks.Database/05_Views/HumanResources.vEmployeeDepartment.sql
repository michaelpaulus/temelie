CREATE VIEW [HumanResources].[vEmployeeDepartment] AS
SELECT
    e.[BusinessEntityID],
    p.[Title],
    p.[FirstName],
    p.[MiddleName],
    p.[LastName],
    p.[Suffix],
    e.[JobTitle],
    d.[Name] AS [Department],
    d.[GroupName],
    edh.[StartDate]
FROM  
    [HumanResources].[Employee] AS e INNER JOIN
    [Person].[Person] AS p ON
         p.[BusinessEntityID] = e.[BusinessEntityID] INNER JOIN
    [HumanResources].[EmployeeDepartmentHistory] AS edh ON
         e.[BusinessEntityID] = edh.[BusinessEntityID] INNER JOIN
    [HumanResources].[Department] AS d ON
         edh.[DepartmentID] = d.[DepartmentID] WHERE
                                                    edh.EndDate IS NULL;
GO