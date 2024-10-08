﻿-- uspGetManagerEmployees
DROP PROCEDURE IF EXISTS [dbo].[uspGetManagerEmployees]
GO

CREATE PROCEDURE [dbo].[uspGetManagerEmployees]
@BusinessEntityID INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH   [EMP_cte] ([BusinessEntityID], [OrganizationNode], [FirstName], [LastName], [RecursionLevel])
    AS     (SELECT
                e.[BusinessEntityID],
                e.[OrganizationNode],
                p.[FirstName],
                p.[LastName],
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
                [RecursionLevel] + 1
            FROM  
                [HumanResources].[Employee] AS e INNER JOIN
                [EMP_cte] ON
                     e.[OrganizationNode].GetAncestor(1) = [EMP_cte].[OrganizationNode] INNER JOIN
                [Person].[Person] AS p ON
                     p.[BusinessEntityID] = e.[BusinessEntityID])
    SELECT
        [EMP_cte].[RecursionLevel],
        [EMP_cte].[OrganizationNode].ToString() AS [OrganizationNode],
        p.[FirstName] AS 'ManagerFirstName',
        p.[LastName] AS 'ManagerLastName',
        [EMP_cte].[BusinessEntityID],
        [EMP_cte].[FirstName],
        [EMP_cte].[LastName]
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
