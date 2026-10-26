/* Chay trong SSMS bang tai khoan co quyen tao database va SQL Login.
   Tai khoan demo cho bai tap: admin / 123 ; user / 123.
   SQL Server Express can bat che do SQL Server and Windows Authentication. */
USE master;
GO
IF DB_ID(N'FashionFlowDb') IS NULL
BEGIN
    DECLARE @DataPath nvarchar(4000) = CONVERT(nvarchar(4000), SERVERPROPERTY(N'InstanceDefaultDataPath'));
    IF NULLIF(@DataPath, N'') IS NULL
    BEGIN
        SELECT @DataPath = LEFT(physical_name, LEN(physical_name) - CHARINDEX(N'\', REVERSE(physical_name)) + 1)
        FROM master.sys.master_files
        WHERE database_id = DB_ID(N'master') AND file_id = 1;
    END;
    IF RIGHT(@DataPath, 1) NOT IN (N'\', N'/') SET @DataPath += N'\';

    /* Dung ten file rieng de khong ghi de MDF/LDF con lai tu lan tao truoc. */
    DECLARE @FileSuffix nvarchar(32) = REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N'');
    DECLARE @DataFile nvarchar(4000) = @DataPath + N'FashionFlowDb_' + @FileSuffix + N'.mdf';
    DECLARE @LogFile nvarchar(4000) = @DataPath + N'FashionFlowDb_' + @FileSuffix + N'_log.ldf';
    DECLARE @CreateDatabaseSql nvarchar(max) =
        N'CREATE DATABASE [FashionFlowDb] ON PRIMARY ' +
        N'(NAME = N''FashionFlowDb_Data'', FILENAME = N''' + REPLACE(@DataFile, N'''', N'''''') + N''') ' +
        N'LOG ON (NAME = N''FashionFlowDb_Log'', FILENAME = N''' + REPLACE(@LogFile, N'''', N'''''') + N''')';
    EXEC sys.sp_executesql @CreateDatabaseSql;
END;
GO
IF SUSER_ID(N'admin') IS NULL
    CREATE LOGIN [admin] WITH PASSWORD = '123', CHECK_POLICY = OFF, DEFAULT_DATABASE = FashionFlowDb;
ELSE IF IS_SRVROLEMEMBER(N'sysadmin') = 1 OR HAS_PERMS_BY_NAME(NULL, NULL, N'ALTER ANY LOGIN') = 1
    ALTER LOGIN [admin] WITH PASSWORD = '123', CHECK_POLICY = OFF, DEFAULT_DATABASE = FashionFlowDb;
IF SUSER_ID(N'user') IS NULL
    CREATE LOGIN [user] WITH PASSWORD = '123', CHECK_POLICY = OFF, DEFAULT_DATABASE = FashionFlowDb;
ELSE IF IS_SRVROLEMEMBER(N'sysadmin') = 1 OR HAS_PERMS_BY_NAME(NULL, NULL, N'ALTER ANY LOGIN') = 1
    ALTER LOGIN [user] WITH PASSWORD = '123', CHECK_POLICY = OFF, DEFAULT_DATABASE = FashionFlowDb;
GO
USE FashionFlowDb;
GO
IF DATABASE_PRINCIPAL_ID(N'admin') IS NULL CREATE USER [admin] FOR LOGIN [admin];
IF DATABASE_PRINCIPAL_ID(N'user') IS NULL CREATE USER [user] FOR LOGIN [user];


IF DATABASE_PRINCIPAL_ID(N'FashionFlowAdmin') IS NULL CREATE ROLE FashionFlowAdmin;
IF DATABASE_PRINCIPAL_ID(N'FashionFlowUser') IS NULL CREATE ROLE FashionFlowUser;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE role_principal_id = DATABASE_PRINCIPAL_ID(N'FashionFlowAdmin') AND member_principal_id = DATABASE_PRINCIPAL_ID(N'admin'))
    ALTER ROLE FashionFlowAdmin ADD MEMBER [admin];
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE role_principal_id = DATABASE_PRINCIPAL_ID(N'FashionFlowUser') AND member_principal_id = DATABASE_PRINCIPAL_ID(N'user'))
    ALTER ROLE FashionFlowUser ADD MEMBER [user];
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE role_principal_id = DATABASE_PRINCIPAL_ID(N'db_owner') AND member_principal_id = DATABASE_PRINCIPAL_ID(N'FashionFlowAdmin'))
    ALTER ROLE db_owner ADD MEMBER FashionFlowAdmin;
GO
/* User duoc doc/ghi schema, nhung khong duoc xoa, tao bang hay sua cau truc. */
REVOKE DELETE ON SCHEMA::dbo FROM FashionFlowUser;
REVOKE CONTROL ON SCHEMA::dbo FROM FashionFlowUser;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo TO FashionFlowUser;
DENY DELETE, ALTER, TAKE OWNERSHIP ON SCHEMA::dbo TO FashionFlowUser;


GO
