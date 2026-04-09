IF OBJECT_ID(N'dbo.Banner', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Banner
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title NVARCHAR(200) NULL,
        SubTitle NVARCHAR(200) NULL,
        Description NVARCHAR(500) NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        LinkUrl NVARCHAR(500) NULL,
        Position NVARCHAR(20) NOT NULL CONSTRAINT DF_Banner_Position DEFAULT('Hero'),
        DisplayOrder INT NOT NULL CONSTRAINT DF_Banner_DisplayOrder DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_Banner_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Banner_CreatedAt DEFAULT(GETDATE())
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Banner)
BEGIN
    INSERT INTO dbo.Banner (Title, SubTitle, Description, ImageUrl, LinkUrl, Position, DisplayOrder, IsActive)
    VALUES
    (N'S?n Ph?m Ch?t L??ng Cao', N'CÔNG NGH? M?I', N'Giao hàng nhanh - Hàng Chính Hãng 100%', N'/img/hero/banner.jpg', N'/Home/Shop', N'Hero', 1, 1),
    (N'Banner 1', N'', N'', N'/img/banner/banner-1.jpg', N'/Home/Shop', N'Bottom', 1, 1),
    (N'Banner 2', N'', N'', N'/img/banner/banner-2.jpg', N'/Home/Shop', N'Bottom', 2, 1);
END;
GO
