using System.Security.Cryptography;

namespace BulletinBoard.Areas.Hanmadi.Services;

public static class SessionCodeGenerator
{
    // 혼동 문자 0 O 1 I 제외
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length = 3)
    {
        var result = new char[length];
        for (int i = 0; i < length; i++)
            result[i] = Chars[RandomNumberGenerator.GetInt32(Chars.Length)];
        return new string(result);
    }
}
