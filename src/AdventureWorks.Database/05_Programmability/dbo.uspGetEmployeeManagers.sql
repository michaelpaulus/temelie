CREATE PROCEDURE [dbo].[uspGetEmployeeManagers]
@BusinessEntityID INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH   [EMP_cte] ([BusinessEntityID], [OrganizationNode], [FirstName], [LastName], [JobTitle], [RecursionLevel])
    AS     (SELECT
                e.[BusinessEntityID],
                e.[OrganizationNode],
                p.[FirstName],
                p.[LastName],
                e.[JobTitle],
                0
            FROM  
                [HumanResources].[Employee] AS e INNER JOIN
                [Person].[Person] AS p ON
                     p.[BusinessEntityID] = e.[BusinessEntityID] WHERE
                                                                      e.[BusinessEntityID] = @BusinessEntityID
            UNION ALL
            SELECT
                e.[BusinessEntityID],
                e.[OrganizationNode],
                p.[FirstName],
                p.[LastName],
                e.[JobTitle],
                [RecursionLevel] + 1
            FROM  
                [HumanResources].[Employee] AS e INNER JOIN
                [EMP_cte] ON
                     e.[OrganizationNode] = [EMP_cte].[OrganizationNode].GetAncestor(1) INNER JOIN
                [Person].[Person] AS p ON
                     p.[BusinessEntityID] = e.[BusinessEntityID])
    SELECT
        [EMP_cte].[RecursionLevel],
        [EMP_cte].[BusinessEntityID],
        [EMP_cte].[FirstName],
        [EMP_cte].[LastName],
        [EMP_cte].[OrganizationNode].ToString() AS [OrganizationNode],
        p.[FirstName] AS 'ManagerFirstName',
        p.[LastName] AS 'ManagerLastName'
    FROM  
        [EMP_cte] INNER JOIN
        [HumanResources].[Employee] AS e ON
             [EMP_cte].[OrganizationNode].GetAncestor(1) = e.[OrganizationNode] INNER JOIN
        [Person].[Person] AS p ON
             p.[BusinessEntityID] = e.[BusinessEntityID] ORDER BY
                                                              [RecursionLevel], [EMP_cte].[OrganizationNode].ToString()
    OPTION (MAXRECURSION 25);
END
GO