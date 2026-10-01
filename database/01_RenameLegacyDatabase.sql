/*
    ATRIUM Academy
    One-time compatibility script for installations created before the technical rename.

    Run this script in SQL Server Management Studio while connected to the local SQL Server
    instance. It renames the existing database without deleting its data.
*/

USE [master];
GO

IF DB_ID(N'ATRIUM_Academy') IS NOT NULL
BEGIN
    PRINT 'ATRIUM_Academy already exists. No rename was performed.';
END
ELSE IF DB_ID(N'VIMOD_Web') IS NOT NULL
BEGIN
    ALTER DATABASE [VIMOD_Web] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [VIMOD_Web] MODIFY NAME = [ATRIUM_Academy];
    ALTER DATABASE [ATRIUM_Academy] SET MULTI_USER;

    PRINT 'Database renamed to ATRIUM_Academy successfully.';
END
ELSE
BEGIN
    PRINT 'No legacy database was found. Create/update ATRIUM_Academy with Entity Framework migrations.';
END
GO
