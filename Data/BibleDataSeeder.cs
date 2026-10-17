using Microsoft.EntityFrameworkCore;

namespace BibleReadingThrough.Data;

public static class BibleDataSeeder
{
    // 필요한 테이블 자동 생성
    public static void EnsureReadingProgressTable(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT * FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[UserBibleProgress]') AND type = 'U'
            )
            BEGIN
                CREATE TABLE [dbo].[UserBibleProgress] (
                    [UserId]  NVARCHAR(450) NOT NULL,
                    [VerseId] INT NOT NULL,
                    [ReadDate] DATETIME NOT NULL CONSTRAINT DF_UserBibleProgress_ReadDate DEFAULT GETDATE(),
                    CONSTRAINT [PK_UserBibleProgress] PRIMARY KEY CLUSTERED ([UserId] ASC, [VerseId] ASC)
                )
            END
        ");

        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT * FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[UserBookMark]') AND type = 'U'
            )
            BEGIN
                CREATE TABLE [dbo].[UserBookMark] (
                    [UserId]   NVARCHAR(450) NOT NULL,
                    [VerseId]  INT NOT NULL,
                    [InsertDT] DATETIME NOT NULL CONSTRAINT DF_UserBookMark_InsertDT DEFAULT GETDATE(),
                    CONSTRAINT [PK_UserBookMark] PRIMARY KEY CLUSTERED ([UserId] ASC, [VerseId] ASC)
                )
            END
        ");
    }

    public static Task SeedAsync(AppDbContext db)
    {
        return Task.CompletedTask;
    }
}
