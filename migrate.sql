IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216104222_DropPermissionsIsActive'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Permissions]') AND [c].[name] = N'IsActive');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Permissions] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Permissions] DROP COLUMN [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216104222_DropPermissionsIsActive'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260216104222_DropPermissionsIsActive', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216140557_TestColumnAdded'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260216140557_TestColumnAdded', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216141804_TempTestFieldAdded'
)
BEGIN
    ALTER TABLE [Customers] ADD [testcolumnnn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216141804_TempTestFieldAdded'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260216141804_TempTestFieldAdded', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218071403_Baseline_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218071403_Baseline_Initial', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218074413_AuthPrep_NoChange'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Permissions]') AND [c].[name] = N'Description');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Permissions] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Permissions] ALTER COLUMN [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218074413_AuthPrep_NoChange'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218074413_AuthPrep_NoChange', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    CREATE TABLE [JobActivities] (
        [JobActivityId] int NOT NULL IDENTITY,
        [JobId] int NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Message] nvarchar(500) NULL,
        [MetaJson] nvarchar(2000) NULL,
        [PerformedByUserId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_JobActivities] PRIMARY KEY ([JobActivityId]),
        CONSTRAINT [FK_JobActivities_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([JobId]) ON DELETE CASCADE,
        CONSTRAINT [FK_JobActivities_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    CREATE TABLE [JobComments] (
        [JobCommentId] int NOT NULL IDENTITY,
        [JobId] int NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [Text] nvarchar(2000) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JobComments] PRIMARY KEY ([JobCommentId]),
        CONSTRAINT [FK_JobComments_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([JobId]) ON DELETE CASCADE,
        CONSTRAINT [FK_JobComments_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    CREATE INDEX [IX_JobActivities_JobId] ON [JobActivities] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    CREATE INDEX [IX_JobActivities_PerformedByUserId] ON [JobActivities] ([PerformedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    CREATE INDEX [IX_JobComments_CreatedByUserId] ON [JobComments] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    CREATE INDEX [IX_JobComments_JobId] ON [JobComments] ([JobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225094434_AddJobCommentAndActivities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260225094434_AddJobCommentAndActivities', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    ALTER TABLE [JobActivities] DROP CONSTRAINT [FK_JobActivities_Users_PerformedByUserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    ALTER TABLE [JobComments] DROP CONSTRAINT [FK_JobComments_Users_CreatedByUserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    IF OBJECT_ID(N'dbo.Jobs', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH('dbo.Jobs', 'Id') IS NOT NULL AND COL_LENGTH('dbo.Jobs', 'JobId') IS NULL
            EXEC sp_rename N'dbo.Jobs.Id', N'JobId', N'COLUMN';
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH('dbo.Customers', 'Id') IS NOT NULL AND COL_LENGTH('dbo.Customers', 'CustomerId') IS NULL
            EXEC sp_rename N'dbo.Customers.Id', N'CustomerId', N'COLUMN';
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobComments]') AND [c].[name] = N'Text');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [JobComments] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [JobComments] ALTER COLUMN [Text] nvarchar(4000) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobActivities]') AND [c].[name] = N'Type');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [JobActivities] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [JobActivities] ALTER COLUMN [Type] nvarchar(100) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobActivities]') AND [c].[name] = N'MetaJson');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [JobActivities] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [JobActivities] ALTER COLUMN [MetaJson] nvarchar(4000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobActivities]') AND [c].[name] = N'Message');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [JobActivities] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [JobActivities] ALTER COLUMN [Message] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [RefreshTokenId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [TokenHash] nvarchar(256) NOT NULL,
        [TokenSalt] nvarchar(256) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([RefreshTokenId]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAt] ON [RefreshTokens] ([UserId], [ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    ALTER TABLE [JobActivities] ADD CONSTRAINT [FK_JobActivities_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    ALTER TABLE [JobComments] ADD CONSTRAINT [FK_JobComments_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070547_Add_RefreshTokens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226070547_Add_RefreshTokens', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226121631_Jobs_Status_ToBit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226121631_Jobs_Status_ToBit', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226122050_Jobs_StatusToBit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226122050_Jobs_StatusToBit', N'8.0.0');
END;
GO

COMMIT;
GO

