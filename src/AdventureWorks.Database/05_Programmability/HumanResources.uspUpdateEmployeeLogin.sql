﻿-- uspUpdateEmployeeLogin
DROP PROCEDURE IF EXISTS [HumanResources].[uspUpdateEmployeeLogin]
GO

CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeLogin]
@BusinessEntityID INT, @OrganizationNode [hierarchyid], @LoginID NVARCHAR (256), @JobTitle NVARCHAR (50), @HireDate DATETIME, @CurrentFlag [dbo].[Flag]
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE [HumanResources].[Employee]
        SET    [OrganizationNode] = @OrganizationNode,
               [LoginID] = @LoginID,
               [JobTitle] = @JobTitle,
               [HireDate] = @HireDate,
               [CurrentFlag] = @CurrentFlag WHERE
                                                 [BusinessEntityID] = @BusinessEntityID;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspLogError] ;
    END CATCH
END
GO
