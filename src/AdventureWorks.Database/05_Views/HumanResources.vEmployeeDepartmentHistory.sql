CREATE VIEW [HumanResources].[vEmployeeDepartmentHistory] AS
SELECT
    e.[BusinessEntityID],
    p.[Title],
    p.[FirstName],
    p.[MiddleName],
    p.[LastName],
    p.[Suffix],
    s.[Name] AS [Shift],
    d.[Name] AS [Department],
    d.[GroupName],
    edh.[StartDate],
    edh.[EndDate]
FROM  
    [HumanResources].[Employee] AS e INNER JOIN
    [Person].[Person] AS p ON
         p.[BusinessEntityID] = e.[BusinessEntityID] INNER JOIN
    [HumanResources].[EmployeeDepartmentHistory] AS edh ON
         e.[BusinessEntityID] = edh.[BusinessEntityID] INNER JOIN
    [HumanResources].[Department] AS d ON
         edh.[DepartmentID] = d.[DepartmentID] INNER JOIN
    [HumanResources].[Shift] AS s ON
         s.[ShiftID] = edh.[ShiftID];
GO