IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HumanResources')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA HumanResources';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Person')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Person';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Production')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Production';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Purchasing')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Purchasing';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Sales')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA Sales';
END
GO
