/* =========================================================
   DATABASE
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'wellcare')
BEGIN
    CREATE DATABASE wellcare;
END
GO

USE wellcare;
GO

/* =========================================================
   TABLES
   ========================================================= */

/* 1. Caretaker Table */
IF OBJECT_ID('caretakerTable', 'U') IS NULL
BEGIN
    CREATE TABLE caretakerTable
    (
        CareTakerID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(256) NOT NULL UNIQUE,
        Phone NVARCHAR(20) NOT NULL UNIQUE,
        Age INT NOT NULL,
        PasswordHash NVARCHAR(256) NOT NULL,
        Photo NVARCHAR(500) NULL,
        HomeAddress NVARCHAR(256) NULL,
        Gender NVARCHAR(20) NOT NULL,
        Bio NVARCHAR(500) NULL,
        IsEmailVerified BIT NOT NULL 
            CONSTRAINT DF_CareTaker_IsEmailVerified DEFAULT (0),
        CreatedAt DATETIME2(3) NOT NULL 
            CONSTRAINT DF_CareTaker_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

/* 2. Elder Table */
IF OBJECT_ID('elderTable', 'U') IS NULL
BEGIN
    CREATE TABLE elderTable
    (
        elderId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        elderName NVARCHAR(50) NOT NULL,
        eldermail NVARCHAR(256) NOT NULL UNIQUE,
        Age INT NOT NULL,
        Gender NVARCHAR(20) NOT NULL,
        PasswordHash NVARCHAR(256) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

/* 3. OTP Table */
IF OBJECT_ID('OTPTable', 'U') IS NULL
BEGIN
    CREATE TABLE OTPTable
    (
        OTPID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CareTakerID INT NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        OTP NVARCHAR(10) NOT NULL,
        Purpose NVARCHAR(100) NOT NULL,
        ExpiresAt DATETIME2(3) NOT NULL,
        IsUsed BIT NOT NULL 
            CONSTRAINT DF_OTP_IsUsed DEFAULT (0),
        CreatedAt DATETIME2(3) NOT NULL 
            CONSTRAINT DF_OTP_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_OTP_CareTaker 
            FOREIGN KEY (CareTakerID) REFERENCES caretakerTable(CareTakerID)
    );
END
GO

/* 4. Caretaker–Elder Map */
IF OBJECT_ID('CaretakerElderMap', 'U') IS NULL
BEGIN
    CREATE TABLE CaretakerElderMap
    (
        CareTakerID INT NOT NULL,
        ElderID INT NOT NULL,
        LinkedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_CaretakerElder 
            PRIMARY KEY (CareTakerID, ElderID),

        CONSTRAINT FK_Map_Caretaker 
            FOREIGN KEY (CareTakerID) REFERENCES caretakerTable(CareTakerID),

        CONSTRAINT FK_Map_Elder 
            FOREIGN KEY (ElderID) REFERENCES elderTable(elderId)
    );
END
GO

/* =========================================================
   STORED PROCEDURES
   ========================================================= */

/* 5. Caretaker Register */
CREATE OR ALTER PROCEDURE sp_caretaker_register
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(256),
    @Phone NVARCHAR(20),
    @Age INT,
    @PasswordHash NVARCHAR(256),
    @HomeAddress NVARCHAR(256) = NULL,
    @Gender NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Status INT = 0;
    DECLARE @CareTakerID INT = 0;

    IF EXISTS (SELECT 1 FROM CareTakerTable WHERE Email = @Email)
    BEGIN
        SELECT -1 AS Status, 0 AS CareTakerID;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM CareTakerTable WHERE Phone = @Phone)
    BEGIN
        SELECT -2 AS Status, 0 AS CareTakerID;
        RETURN;
    END

    BEGIN TRY
        INSERT INTO CareTakerTable
        (
            FirstName, LastName, Email, Phone, Age,
            PasswordHash, HomeAddress, Gender,
            IsEmailVerified, CreatedAt
        )
        VALUES
        (
            @FirstName, @LastName, @Email, @Phone, @Age,
            @PasswordHash, @HomeAddress, @Gender,
            0, SYSUTCDATETIME()
        );

        SET @CareTakerID = SCOPE_IDENTITY();
        SET @Status = 1;
    END TRY
    BEGIN CATCH
        SET @Status = -500;
    END CATCH

    SELECT @Status AS Status, @CareTakerID AS CareTakerID;
END
GO

/* 6. Caretaker Login */
CREATE OR ALTER PROCEDURE sp_caretaker_login
    @loginInput NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CareTakerID, FirstName, PasswordHash, IsEmailVerified
    FROM CareTakerTable
    WHERE Email = @loginInput;
END
GO

/* 7. OTP Create */
CREATE OR ALTER PROCEDURE sp_otp
    @CareTakerID INT,
    @Email NVARCHAR(256),
    @OTP NVARCHAR(10),
    @Purpose NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO OTPTable
    (CareTakerID, Email, OTP, Purpose, ExpiresAt, IsUsed, CreatedAt)
    VALUES
    (@CareTakerID, @Email, @OTP, @Purpose,
     DATEADD(MINUTE, 10, SYSUTCDATETIME()), 0, SYSUTCDATETIME());
END
GO

/* 8. OTP Verify (Email) */
CREATE OR ALTER PROCEDURE sp_otp_verify
    @Email NVARCHAR(256),
    @OTP NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CareTakerID INT;

    SELECT @CareTakerID = CareTakerID
    FROM OTPTable
    WHERE Email = @Email
      AND OTP = @OTP
      AND IsUsed = 0
      AND ExpiresAt > SYSUTCDATETIME();

    IF @CareTakerID IS NULL
    BEGIN
        DELETE FROM OTPTable WHERE Email = @Email;
        DELETE FROM CareTakerTable WHERE Email = @Email AND IsEmailVerified = 0;
        SELECT -1 AS Status;
        RETURN;
    END

    UPDATE CareTakerTable SET IsEmailVerified = 1 WHERE CareTakerID = @CareTakerID;
    UPDATE OTPTable SET IsUsed = 1 WHERE CareTakerID = @CareTakerID;

    SELECT 1 AS Status;
END
GO

/* 9. OTP Resend */
CREATE OR ALTER PROCEDURE sp_otp_resend
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CareTakerID INT;
    DECLARE @OTP NVARCHAR(6);

    SELECT @CareTakerID = CareTakerID
    FROM caretakerTable
    WHERE Email = @Email AND IsEmailVerified = 0;

    IF @CareTakerID IS NULL
    BEGIN
        SELECT -1 AS Status;
        RETURN;
    END

    UPDATE OTPTable SET IsUsed = 1 WHERE Email = @Email;

    SET @OTP = CAST(ABS(CHECKSUM(NEWID())) % 900000 + 100000 AS NVARCHAR(6));

    INSERT INTO OTPTable
    (CareTakerID, Email, OTP, Purpose, ExpiresAt, IsUsed, CreatedAt)
    VALUES
    (@CareTakerID, @Email, @OTP, 'CaretakerEmailVerification',
     DATEADD(MINUTE, 10, SYSUTCDATETIME()), 0, SYSUTCDATETIME());

    SELECT 1 AS Status, @OTP AS OTP;
END
GO

/* 10. OTP Password Reset */
CREATE OR ALTER PROCEDURE sp_otp_password_reset
    @Email NVARCHAR(256),
    @OTP NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CareTakerID INT;

    SELECT @CareTakerID = CareTakerID FROM caretakerTable WHERE Email = @Email;
    IF @CareTakerID IS NULL RETURN;

    UPDATE OTPTable SET IsUsed = 1 WHERE Email = @Email;

    INSERT INTO OTPTable
    (CareTakerID, Email, OTP, Purpose, ExpiresAt, IsUsed, CreatedAt)
    VALUES
    (@CareTakerID, @Email, @OTP, 'PasswordReset',
     DATEADD(MINUTE, 10, SYSUTCDATETIME()), 0, SYSUTCDATETIME());
END
GO

/* 11. Password Reset Verify */
CREATE OR ALTER PROCEDURE sp_password_reset_verify
    @Email NVARCHAR(256),
    @OTP NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM OTPTable
        WHERE Email = @Email AND OTP = @OTP
          AND Purpose = 'PasswordReset'
          AND IsUsed = 0
          AND ExpiresAt > SYSUTCDATETIME()
    )
    BEGIN
        SELECT -1 AS Status;
        RETURN;
    END

    UPDATE OTPTable
    SET IsUsed = 1
    WHERE Email = @Email AND OTP = @OTP AND Purpose = 'PasswordReset';

    SELECT 1 AS Status;
END
GO

/* 12. Assign Elder to Caretaker */
CREATE OR ALTER PROCEDURE sp_assign_elder_to_caretaker
    @CareTakerID INT,
    @ElderEmail NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ElderID INT;

    SELECT @ElderID = elderId
    FROM elderTable
    WHERE eldermail = @ElderEmail AND IsActive = 1;

    IF @ElderID IS NULL
    BEGIN
        SELECT -1 AS Status;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM CaretakerElderMap WHERE ElderID = @ElderID)
    BEGIN
        SELECT -2 AS Status;
        RETURN;
    END

    INSERT INTO CaretakerElderMap (CareTakerID, ElderID)
    VALUES (@CareTakerID, @ElderID);

    SELECT 1 AS Status;
END
GO
