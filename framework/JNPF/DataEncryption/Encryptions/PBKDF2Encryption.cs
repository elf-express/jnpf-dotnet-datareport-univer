﻿using System.Security.Cryptography;

namespace JNPF.DataEncryption;

/// <summary>
/// PBKDF2 加密
/// </summary>
[SuppressSniffer]
public class PBKDF2Encryption
{
    private const string SaltHashSeparator = ":";

    /// <summary>
    /// PBKDF2 加密
    /// </summary>
    /// <param name="text">加密文本</param>
    /// <param name="saltSize">随机 salt 大小</param>
    /// <param name="iterationCount">迭代次数</param>
    /// <param name="derivedKeyLength">密钥长度</param>
    /// <returns></returns>
    public static string Encrypt(string text, int saltSize = 16, int iterationCount = 10000, int derivedKeyLength = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[saltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(text, salt, iterationCount, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(derivedKeyLength);

        // 分别编码盐和哈希，并用分隔符拼接
        return Convert.ToBase64String(salt) + SaltHashSeparator + Convert.ToBase64String(hash);
    }

    /// <summary>
    /// PBKDF2 比较
    /// </summary>
    /// <param name="text">加密文本</param>
    /// <param name="hash">PBKDF2 字符串</param>
    /// <param name="saltSize">随机 salt 大小</param>
    /// <param name="iterationCount">迭代次数</param>
    /// <param name="derivedKeyLength">密钥长度</param>
    /// <returns></returns>
    public static bool Compare(string text, string hash, int saltSize = 16, int iterationCount = 10000, int derivedKeyLength = 32)
    {
        try
        {
            var parts = hash.Split(new[] { SaltHashSeparator }, StringSplitOptions.None);
            if (parts.Length != 2)
                return false;

            var saltBytes = Convert.FromBase64String(parts[0]);
            var storedHashBytes = Convert.FromBase64String(parts[1]);

            if (saltBytes.Length != saltSize || storedHashBytes.Length != derivedKeyLength)
                return false;

            using var pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, iterationCount, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(derivedKeyLength);

            return computedHash.SequenceEqual(storedHashBytes);
        }
        catch
        {
            return false;
        }
    }
}
