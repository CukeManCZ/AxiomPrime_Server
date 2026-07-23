using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class CryptoUtils
{
    // ⚠️ MUST be 16 / 24 / 32 bytes for AES
    // In real production: load from secure config, NOT hardcoded
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456");

    /// <summary>
    /// Encrypts plain text using AES
    /// Output = IV + Cipher (Base64)
    /// </summary>
    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using ICryptoTransform encryptor = aes.CreateEncryptor();

        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];

        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Decrypts AES encrypted Base64 string
    /// </summary>
    public static string Decrypt(string encryptedText)
    {
        byte[] fullBytes = Convert.FromBase64String(encryptedText);

        using Aes aes = Aes.Create();
        aes.Key = Key;

        byte[] iv = new byte[16];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, 16);
        aes.IV = iv;

        byte[] cipher = new byte[fullBytes.Length - 16];
        Buffer.BlockCopy(fullBytes, 16, cipher, 0, cipher.Length);

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}