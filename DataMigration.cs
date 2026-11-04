using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace BarangayManagementSystem
{
    public static class DataMigration
    {
        public static int EncryptExistingResidentData()
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                if (connection == null)
                    throw new InvalidOperationException("The local database could not be opened.");

                byte[] key = KeyManager.GetOrCreateKey();
                using (var transaction = connection.BeginTransaction())
                {
                    var residents = new List<ResidentFields>();
                    using (var command = new SQLiteCommand(
                        "SELECT idResident, ContactNumber, Address, TIN FROM resident",
                        connection, transaction))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string contactNumber = ReadNullableString(reader, "ContactNumber");
                            string address = ReadNullableString(reader, "Address");
                            string tin = ReadNullableString(reader, "TIN");

                            bool updated = false;
                            contactNumber = EncryptIfPlaintext(contactNumber, key, ref updated);
                            address = EncryptIfPlaintext(address, key, ref updated);
                            tin = EncryptIfPlaintext(tin, key, ref updated);

                            if (updated)
                            {
                                residents.Add(new ResidentFields
                                {
                                    Id = Convert.ToInt32(reader["idResident"]),
                                    ContactNumber = contactNumber,
                                    Address = address,
                                    Tin = tin
                                });
                            }
                        }
                    }

                    foreach (var resident in residents)
                    {
                        using (var command = new SQLiteCommand(@"
                            UPDATE resident
                            SET ContactNumber=@contactNumber, Address=@address, TIN=@tin
                            WHERE idResident=@id", connection, transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@contactNumber", DbValue(resident.ContactNumber));
                            command.Parameters.AddWithValue(
                                "@address", DbValue(resident.Address));
                            command.Parameters.AddWithValue("@tin", DbValue(resident.Tin));
                            command.Parameters.AddWithValue("@id", resident.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return residents.Count;
                }
            }
        }

        private static string ReadNullableString(IDataRecord reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static string EncryptIfPlaintext(string value, byte[] key, ref bool updated)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            try
            {
                CryptoHelper.Decrypt(value, key);
                return value;
            }
            catch (CryptoException)
            {
                updated = true;
                return CryptoHelper.Encrypt(value, key);
            }
        }

        private static object DbValue(string value)
        {
            return value == null ? (object)DBNull.Value : value;
        }

        private sealed class ResidentFields
        {
            public int Id { get; set; }
            public string ContactNumber { get; set; }
            public string Address { get; set; }
            public string Tin { get; set; }
        }
    }
}
