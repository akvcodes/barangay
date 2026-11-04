using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BarangayManagementSystem
{
    public static class CryptoTest
    {
        public static void RunSelfTest()
        {
            byte[] key = KeyManager.GetOrCreateKey();
            const string originalText = "Hello Barangay";

            string cipherText = CryptoHelper.Encrypt(originalText, key);
            WriteOutput("Ciphertext: " + cipherText);

            string decryptedText = CryptoHelper.Decrypt(cipherText, key);
            WriteOutput("Plaintext: " + decryptedText);

            if (!string.Equals(originalText, decryptedText, StringComparison.Ordinal))
                throw new InvalidOperationException("Encryption round-trip did not match.");

            WriteOutput("Round-trip: OK");

            try
            {
                CryptoHelper.Decrypt("not-encrypted-plaintext", key);
                throw new InvalidOperationException(
                    "Legacy plaintext was not rejected by the decryptor.");
            }
            catch (CryptoException)
            {
                WriteOutput("Legacy detection: OK");
            }

            string keyPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "key.dat");
            byte[] previousKeyFile = File.Exists(keyPath)
                ? File.ReadAllBytes(keyPath)
                : null;

            try
            {
                if (File.Exists(keyPath))
                    File.Delete(keyPath);

                byte[] regeneratedKey = KeyManager.GetOrCreateKey();
                if (regeneratedKey.Length != 32 ||
                    regeneratedKey.SequenceEqual(key))
                {
                    throw new InvalidOperationException(
                        "Key regeneration did not produce a new 32-byte key.");
                }

                WriteOutput("Key regeneration: OK");
            }
            finally
            {
                if (previousKeyFile != null)
                    File.WriteAllBytes(keyPath, previousKeyFile);
            }
        }

        private static void WriteOutput(string message)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }
    }
}
