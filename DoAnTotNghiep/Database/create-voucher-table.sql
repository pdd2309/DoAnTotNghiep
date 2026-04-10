IF OBJECT_ID(N'dbo.Voucher', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Voucher
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Code VARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NULL,
        DiscountType VARCHAR(20) NOT NULL CONSTRAINT DF_Voucher_DiscountType DEFAULT('Amount'),
        DiscountValue DECIMAL(18,2) NOT NULL,
        MaxDiscountAmount DECIMAL(18,2) NULL,
        MinOrderAmount DECIMAL(18,2) NULL,
        Quantity INT NOT NULL CONSTRAINT DF_Voucher_Quantity DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_Voucher_IsActive DEFAULT(1),
        StartDate DATETIME NULL,
        EndDate DATETIME NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Voucher_CreatedAt DEFAULT(GETDATE())
    );

    CREATE UNIQUE INDEX UQ_Voucher_Code ON dbo.Voucher(Code);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Voucher WHERE Code = 'WELCOME10')
BEGIN
    INSERT INTO dbo.Voucher
    (
        Code, Name, DiscountType, DiscountValue, MaxDiscountAmount,
        MinOrderAmount, Quantity, IsActive, StartDate, EndDate
    )
    VALUES
    (
        'WELCOME10', N'Giam 10% toi da 100000', 'Percent', 10, 100000,
        200000, 100, 1, GETDATE(), DATEADD(MONTH, 6, GETDATE())
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Voucher WHERE Code = 'GIAM50K')
BEGIN
    INSERT INTO dbo.Voucher
    (
        Code, Name, DiscountType, DiscountValue, MaxDiscountAmount,
        MinOrderAmount, Quantity, IsActive, StartDate, EndDate
    )
    VALUES
    (
        'GIAM50K', N'Giam truc tiep 50000', 'Amount', 50000, NULL,
        300000, 100, 1, GETDATE(), DATEADD(MONTH, 6, GETDATE())
    );
END;
GO
