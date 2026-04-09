IF COL_LENGTH('dbo.DanhMuc', 'HinhAnh') IS NULL
BEGIN
    ALTER TABLE dbo.DanhMuc
    ADD HinhAnh NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH('dbo.DanhMuc', 'IsHienThiTrangChu') IS NULL
BEGIN
    ALTER TABLE dbo.DanhMuc
    ADD IsHienThiTrangChu BIT NOT NULL CONSTRAINT DF_DanhMuc_IsHienThiTrangChu DEFAULT(1);
END;
GO

IF COL_LENGTH('dbo.DanhMuc', 'ThuTuHienThi') IS NULL
BEGIN
    ALTER TABLE dbo.DanhMuc
    ADD ThuTuHienThi INT NOT NULL CONSTRAINT DF_DanhMuc_ThuTuHienThi DEFAULT(0);
END;
GO

-- Optional sample values
-- UPDATE dbo.DanhMuc SET HinhAnh = '/img/categories/cat-1.jpg' WHERE MaDanhMuc = 1;
-- UPDATE dbo.DanhMuc SET HinhAnh = '/img/categories/cat-2.jpg' WHERE MaDanhMuc = 2;
