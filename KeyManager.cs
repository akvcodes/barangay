using System;
using System.IO;
using System.Security.Cryptography;

namespace BarangayManagementSystem
{
    public static class KeyManager
    {
        private const int KeySizeBytes = 32;
        private static readonly object SyncRoot = new object();

        private static string KeyFilePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.dat");

        public static byte[] GetOrCreateKey()
        {
            lock (SyncRoot)
            {
                if (File.Exists(KeyFilePath))
                    return LoadKey();

                byte[] key = new byte[KeySizeBytes];
                using (var random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(key);
                }

                byte[] protectedKey = ProtectedData.Protect(
                    key, null, DataProtectionScope.CurrentUser);

                FileStream stream;
                try
                {
                    stream = new FileStream(
                        KeyFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                }
                catch (IOException) when (File.Exists(KeyFilePath))
                {
                    return LoadKey();
                }

                using (stream)
                {
                    stream.Write(protectedKey, 0, protectedKey.Length);
                }

                return key;
            }
        }

        public static byte[] LoadKey()
        {
            if (!File.Exists(KeyFilePath))
            {
                throw new FileNotFoundException(
                    "The encryption key file was not found. Create it with GetOrCreateKey before decrypting data.",
                    KeyFilePath);
            }

            try
            {
                byte[] protectedKey = File.ReadAllBytes(KeyFilePath);
                byte[] key = ProtectedData.Unprotect(
                    protectedKey, null, DataProtectionScope.CurrentUser);

                if (key.Length != KeySizeBytes)
                {
                    throw new InvalidDataException(
                        "The encryption key file did not contain a valid 32-byte key.");
                }

                return key;
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException(
                    "The encryption key could not be unprotected for the current Windows user. " +
                    "Use the same Windows account and profile that created the key.",
                    ex);
            }
        }
    }
}
