/*
    Create extension tables:
    - AddressBook
    - PaymentTransaction
    - OrderStatusHistory
    - VoucherUsage
*/

IF OBJECT_ID(N'dbo.AddressBook', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AddressBook
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaNguoiDung INT NOT NULL,
        FullName NVARCHAR(150) NULL,
        Phone VARCHAR(20) NULL,
        AddressLine NVARCHAR(250) NULL,
        Ward NVARCHAR(100) NULL,
        District NVARCHAR(100) NULL,
        Province NVARCHAR(100) NULL,
        IsDefault BIT NOT NULL CONSTRAINT DF_AddressBook_IsDefault DEFAULT(0),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AddressBook_CreatedAt DEFAULT(GETDATE())
    );

    ALTER TABLE dbo.AddressBook
    ADD CONSTRAINT FK_AddressBook_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung)
        ON DELETE CASCADE;
END;
GO

IF OBJECT_ID(N'dbo.PaymentTransaction', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentTransaction
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaDonHang INT NOT NULL,
        Provider VARCHAR(30) NOT NULL,
        TransactionNo VARCHAR(100) NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status VARCHAR(30) NOT NULL CONSTRAINT DF_PaymentTransaction_Status DEFAULT('Pending'),
        ResponseCode VARCHAR(20) NULL,
        PaidAt DATETIME NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_PaymentTransaction_CreatedAt DEFAULT(GETDATE())
    );

    ALTER TABLE dbo.PaymentTransaction
    ADD CONSTRAINT FK_PaymentTransaction_DonHang
        FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang)
        ON DELETE CASCADE;

    CREATE INDEX IX_PaymentTransaction_MaDonHang ON dbo.PaymentTransaction(MaDonHang);
    CREATE INDEX IX_PaymentTransaction_TransactionNo ON dbo.PaymentTransaction(TransactionNo);
END;
GO

IF OBJECT_ID(N'dbo.OrderStatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderStatusHistory
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaDonHang INT NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        ChangedByUserId INT NULL,
        Note NVARCHAR(500) NULL,
        ChangedAt DATETIME NOT NULL CONSTRAINT DF_OrderStatusHistory_ChangedAt DEFAULT(GETDATE())
    );

    ALTER TABLE dbo.OrderStatusHistory
    ADD CONSTRAINT FK_OrderStatusHistory_DonHang
        FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang)
        ON DELETE CASCADE;

    ALTER TABLE dbo.OrderStatusHistory
    ADD CONSTRAINT FK_OrderStatusHistory_NguoiDung
        FOREIGN KEY (ChangedByUserId) REFERENCES dbo.NguoiDung(MaNguoiDung)
        ON DELETE SET NULL;

    CREATE INDEX IX_OrderStatusHistory_MaDonHang ON dbo.OrderStatusHistory(MaDonHang);
END;
GO

IF OBJECT_ID(N'dbo.VoucherUsage', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VoucherUsage
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        VoucherId INT NOT NULL,
        MaNguoiDung INT NOT NULL,
        MaDonHang INT NOT NULL,
        VoucherCode VARCHAR(50) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL,
        UsedAt DATETIME NOT NULL CONSTRAINT DF_VoucherUsage_UsedAt DEFAULT(GETDATE())
    );

    ALTER TABLE dbo.VoucherUsage
    ADD CONSTRAINT FK_VoucherUsage_Voucher
        FOREIGN KEY (VoucherId) REFERENCES dbo.Voucher(Id);

    ALTER TABLE dbo.VoucherUsage
    ADD CONSTRAINT FK_VoucherUsage_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung);

    ALTER TABLE dbo.VoucherUsage
    ADD CONSTRAINT FK_VoucherUsage_DonHang
        FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang)
        ON DELETE CASCADE;

    CREATE UNIQUE INDEX UQ_VoucherUsage_MaDonHang ON dbo.VoucherUsage(MaDonHang);
    CREATE INDEX IX_VoucherUsage_VoucherId ON dbo.VoucherUsage(VoucherId);
    CREATE INDEX IX_VoucherUsage_MaNguoiDung ON dbo.VoucherUsage(MaNguoiDung);
END;
GO
