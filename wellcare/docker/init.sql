-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WellCareDB')
BEGIN
    CREATE DATABASE WellCareDB;
END
GO

USE WellCareDB;
GO

-- Example table (temporary test)
CREATE TABLE TestTable (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100)
);
GO
