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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102115117_InitialMigration'
)
BEGIN
    CREATE TABLE [SmsChallenges] (
        [Id] uniqueidentifier NOT NULL,
        [SubjectId] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [CodeMac] nvarchar(max) NULL,
        [ExpiresAt] datetimeoffset NULL,
        [FailedAttempts] int NULL,
        [IssuedAt] datetimeoffset NULL,
        [VerifiedAt] datetimeoffset NULL,
        CONSTRAINT [PK_SmsChallenges] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102115117_InitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102115117_InitialMigration', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103115310_AddingSigningKeyEntity'
)
BEGIN
    CREATE TABLE [SigningKeys] (
        [Id] uniqueidentifier NOT NULL,
        [KeyId] nvarchar(max) NOT NULL,
        [Algorithm] nvarchar(max) NOT NULL,
        [PublicKeyPath] nvarchar(max) NOT NULL,
        [PrivateKeyPath] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [RetiredAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SigningKeys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103115310_AddingSigningKeyEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261103115310_AddingSigningKeyEntity', N'10.0.14');
END;

COMMIT;
GO

