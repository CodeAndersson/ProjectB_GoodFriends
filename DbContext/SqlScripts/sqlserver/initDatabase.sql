USE [sql-friends];
GO

--create a schemas
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'gstusr')
    EXEC('CREATE SCHEMA gstusr');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'usr')
    EXEC('CREATE SCHEMA usr');
GO

--create a view that gives overview of the database content
CREATE OR ALTER VIEW gstusr.vwInfoDb AS
    SELECT (SELECT COUNT(*) FROM supusr.Friends WHERE Seeded = 1) as nrSeededFriends, 
        (SELECT COUNT(*) FROM supusr.Friends WHERE Seeded = 0) as nrUnseededFriends,
        (SELECT COUNT(*) FROM supusr.Friends WHERE AddressId IS NOT NULL) as nrFriendsWithAddress,
        (SELECT COUNT(*) FROM supusr.Addresses WHERE Seeded = 1) as nrSeededAddresses, 
        (SELECT COUNT(*) FROM supusr.Addresses WHERE Seeded = 0) as nrUnseededAddresses,
        (SELECT COUNT(*) FROM supusr.Pets WHERE Seeded = 1) as nrSeededPets, 
        (SELECT COUNT(*) FROM supusr.Pets WHERE Seeded = 0) as nrUnseededPets,
        (SELECT COUNT(*) FROM supusr.Quotes WHERE Seeded = 1) as nrSeededQuotes, 
        (SELECT COUNT(*) FROM supusr.Quotes WHERE Seeded = 0) as nrUnseededQuotes;
GO

--create views with rollups
CREATE OR ALTER VIEW gstusr.vwInfoFriends AS
    SELECT a.Country, a.City, COUNT(*) as nrFriends
    FROM supusr.Friends f
    INNER JOIN supusr.Addresses a ON f.AddressId = a.AddressId
    GROUP BY ROLLUP(a.Country, a.City);
GO

CREATE OR ALTER VIEW gstusr.vwInfoPets AS
    SELECT a.Country, a.City, COUNT(p.PetId) as nrPets
    FROM supusr.Friends f
    INNER JOIN supusr.Addresses a ON f.AddressId = a.AddressId
    INNER JOIN supusr.Pets p ON p.FriendId = f.FriendId
    GROUP BY ROLLUP(a.Country, a.City);
GO

CREATE OR ALTER VIEW gstusr.vwInfoQuotes AS
    SELECT Author, COUNT(QuoteText) as nrQuotes
    FROM supusr.Quotes
    GROUP BY Author;
GO

--create the DeleteAll procedure
CREATE OR ALTER PROC supusr.spDeleteAll
    @seededParam BIT = 1,
    @nrFriendsAffected INT OUTPUT,
    @nrAddressesAffected INT OUTPUT,
    @nrPetsAffected INT OUTPUT,
    @nrQuotesAffected INT OUTPUT
AS

    SET NOCOUNT ON;

    SELECT  @nrFriendsAffected = COUNT(*) FROM supusr.Friends WHERE Seeded = @seededParam;
    SELECT  @nrAddressesAffected = COUNT(*) FROM supusr.Addresses WHERE Seeded = @seededParam;
    SELECT  @nrPetsAffected = COUNT(*) FROM supusr.Pets WHERE Seeded = @seededParam;
    SELECT  @nrQuotesAffected = COUNT(*) FROM supusr.Quotes WHERE Seeded = @seededParam;

    DELETE FROM supusr.Friends WHERE Seeded = @seededParam;
    DELETE FROM supusr.Addresses WHERE Seeded = @seededParam;
    DELETE FROM supusr.Pets WHERE Seeded = @seededParam;
    DELETE FROM supusr.Quotes WHERE Seeded = @seededParam;

    --throw our own error
    --;THROW 999999, 'Error occurred in supusr.spDeleteAll', 1

    SELECT * FROM gstusr.vwInfoDb;
GO

-- ------------------------------------------------------------
-- Login procedure used by DbRepos/LoginDbRepos.cs
-- ------------------------------------------------------------
CREATE OR ALTER PROC gstusr.spLogin
    @UserNameOrEmail VARCHAR(100),
    @UserPassword VARCHAR(200),
    @UserId UNIQUEIDENTIFIER OUTPUT,
    @UserName VARCHAR(100) OUTPUT,
    @UserRole VARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- This project now uses ASP.NET Core Identity tables (AspNetUsers etc.).
    -- The legacy dbo.Users table may not exist; in that case this procedure returns NULLs.
    IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    BEGIN
        SELECT TOP 1
            @UserId = u.UserId,
            @UserName = u.UserName,
            @UserRole = u.UserRole
        FROM dbo.Users u
        WHERE (u.UserName = @UserNameOrEmail OR u.Email = @UserNameOrEmail)
          AND u.Password = @UserPassword;
    END
    ELSE
    BEGIN
        SELECT @UserId = NULL, @UserName = NULL, @UserRole = NULL;
    END
END
GO

-- ------------------------------------------------------------
-- Users, logins and roles
-- ------------------------------------------------------------
-- Create server logins (if missing)
IF SUSER_ID (N'gstusr') IS NULL
    CREATE LOGIN gstusr WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;
ELSE
    ALTER LOGIN gstusr WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;

IF SUSER_ID (N'usr') IS NULL
    CREATE LOGIN usr WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;
ELSE
    ALTER LOGIN usr WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;

IF SUSER_ID (N'supusr') IS NULL
    CREATE LOGIN supusr WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;
ELSE
    ALTER LOGIN supusr WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;

IF SUSER_ID (N'dbo') IS NULL
    CREATE LOGIN dbo WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;
ELSE
    ALTER LOGIN dbo WITH PASSWORD = 'pa$Word1', CHECK_POLICY = OFF;
GO

-- Create database users mapped to logins
IF USER_ID(N'gstusrUser') IS NULL
    CREATE USER gstusrUser FOR LOGIN gstusr WITH DEFAULT_SCHEMA = gstusr;
IF USER_ID(N'usrUser') IS NULL
    CREATE USER usrUser FOR LOGIN usr WITH DEFAULT_SCHEMA = usr;
IF USER_ID(N'supusrUser') IS NULL
    CREATE USER supusrUser FOR LOGIN supusr WITH DEFAULT_SCHEMA = supusr;
IF USER_ID(N'dboUser') IS NULL
    CREATE USER dboUser FOR LOGIN dbo WITH DEFAULT_SCHEMA = dbo;
GO

-- Create roles
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'gstUsrRole' AND type = 'R')
    CREATE ROLE gstUsrRole;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'usrRole' AND type = 'R')
    CREATE ROLE usrRole;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'supUsrRole' AND type = 'R')
    CREATE ROLE supUsrRole;
GO

-- Role membership
ALTER ROLE gstUsrRole ADD MEMBER gstusrUser;
ALTER ROLE gstUsrRole ADD MEMBER usrUser;
ALTER ROLE gstUsrRole ADD MEMBER supusrUser;

ALTER ROLE usrRole ADD MEMBER usrUser;
ALTER ROLE usrRole ADD MEMBER supusrUser;

ALTER ROLE supUsrRole ADD MEMBER supusrUser;

-- dbo gets full db_owner
ALTER ROLE db_owner ADD MEMBER dboUser;
GO

-- Grants
GRANT SELECT ON OBJECT::gstusr.vwInfoDb TO gstUsrRole;
GRANT SELECT ON OBJECT::gstusr.vwInfoFriends TO gstUsrRole;
GRANT SELECT ON OBJECT::gstusr.vwInfoPets TO gstUsrRole;
GRANT SELECT ON OBJECT::gstusr.vwInfoQuotes TO gstUsrRole;
GRANT EXECUTE ON OBJECT::gstusr.spLogin TO gstUsrRole;

GRANT SELECT, INSERT, UPDATE ON OBJECT::supusr.Addresses TO usrRole;
GRANT SELECT, INSERT, UPDATE ON OBJECT::supusr.Friends TO usrRole;
GRANT SELECT, INSERT, UPDATE ON OBJECT::supusr.Pets TO usrRole;
GRANT SELECT, INSERT, UPDATE ON OBJECT::supusr.Quotes TO usrRole;
GRANT SELECT, INSERT, UPDATE ON OBJECT::supusr.FriendDbMQuoteDbM TO usrRole;

GRANT DELETE ON OBJECT::supusr.Addresses TO supUsrRole;
GRANT DELETE ON OBJECT::supusr.Friends TO supUsrRole;
GRANT DELETE ON OBJECT::supusr.Pets TO supUsrRole;
GRANT DELETE ON OBJECT::supusr.Quotes TO supUsrRole;
GRANT DELETE ON OBJECT::supusr.FriendDbMQuoteDbM TO supUsrRole;
GRANT EXECUTE ON OBJECT::supusr.spDeleteAll TO supUsrRole;
GO