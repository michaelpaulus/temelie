﻿-- uspUpdateEmployeeHireInfo
DROP PROCEDURE IF EXISTS [HumanResources].[uspUpdateEmployeeHireInfo]
GO

CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
@BusinessEntityID INT, @JobTitle NVARCHAR (50), @HireDate DATETIME, @RateChangeDate DATETIME, @Rate MONEY, @PayFrequency TINYINT, @CurrentFlag [dbo].[Flag]
WITH EXECUTE AS CALLER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        UPDATE [HumanResources].[Employee]
        SET    [JobTitle] = @JobTitle,
               [HireDate] = @HireDate,
               [CurrentFlag] = @CurrentFlag WHERE
                                                 [BusinessEntityID] = @BusinessEntityID;
        INSERT  INTO [HumanResources].[EmployeePayHistory] ([BusinessEntityID], [RateChangeDate], [Rate], [PayFrequency])
        VALUES                                            (@BusinessEntityID, @RateChangeDate, @Rate, @PayFrequency);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            BEGIN
                ROLLBACK;
            END
        EXECUTE [dbo].[uspLogError] ;
    END CATCH
END
GO
