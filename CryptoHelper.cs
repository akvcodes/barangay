using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BarangayManagementSystem
{
    public static class CryptoHelper
    {
        private const int KeySizeBytes = 32;
        private const int IvSizeBytes = 16;

        public static string Encrypt(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            ValidateKey(key);

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var output = new MemoryStream())
                {
                    output.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(
                        output, encryptor, CryptoStreamMode.Write, true))
                    using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8))
                    {
                        writer.Write(plainText);
                    }

                    return Convert.ToBase64String(output.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            ValidateKey(key);

            try
            {
                byte[] payload = Convert.FromBase64String(cipherText);
                if (payload.Length <= IvSizeBytes ||
                    (payload.Length - IvSizeBytes) % IvSizeBytes != 0)
                {
                    throw new CryptoException("The encrypted value has an invalid format.");
                }

                byte[] iv = new byte[IvSizeBytes];
                byte[] encryptedBytes = new byte[payload.Length - IvSizeBytes];
                Buffer.BlockCopy(payload, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(payload, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var input = new MemoryStream(encryptedBytes))
                    using (var cryptoStream = new CryptoStream(
                        input, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cryptoStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (CryptoException)
            {
                throw;
            }
            catch (Exception ex) when (
                ex is FormatException ||
                ex is CryptographicException ||
                ex is DecoderFallbackException)
            {
                throw new CryptoException(
                    "The value could not be decrypted. It may be plaintext or encrypted with a different key.",
                    ex);
            }
        }

        private static void ValidateKey(byte[] key)
        {
            if (key == null || key.Length != KeySizeBytes)
                throw new ArgumentException("AES-256 requires a 32-byte key.", nameof(key));
        }
    }

    public sealed class CryptoException : Exception
    {
        public CryptoException(string message)
            : base(message)
        {
        }

        public CryptoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
