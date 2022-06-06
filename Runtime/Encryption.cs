using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace pow.aidkit
{
    /// <summary>
    ///     Exception handling class
    /// </summary>
    public class CryptoHelpException : ApplicationException
    {
        public CryptoHelpException(string msg) : base(msg)
        {
        }
    }

    public static class Encryption
    {
        private const ulong FcTag = 0xFC010203040506CF;
        private const int BufferSize = 128 * 1024;

        /// <summary>
        ///     Encrypted file random number generation
        /// </summary>
        private static readonly RandomNumberGenerator Rand = new RNGCryptoServiceProvider();

        /// <summary>
        ///     Check whether two Byte arrays are the same
        /// </summary>
        /// <param name="b1">Byte array</param>
        /// <param name="b2">Byte array</param>
        /// <returns>true-equal</returns>
        private static bool CheckByteArrays(IReadOnlyCollection<byte> b1, IReadOnlyList<byte> b2)
        {
            if (b1.Count == b2.Count) return !b1.Where((t, i) => t != b2[i]).Any();

            return false;
        }

        /// <summary>
        ///     encryption
        /// </summary>
        /// <param name="password">password</param>
        /// <param name="salt"></param>
        /// <returns>encrypted objects</returns>
        private static SymmetricAlgorithm CreateRijndael(string password, byte[] salt)
        {
            var pdb = new Rfc2898DeriveBytes(password, salt, 1000);

            SymmetricAlgorithm sma = Rijndael.Create();
            sma.KeySize = 256;
            sma.Key = pdb.GetBytes(32);
            sma.Padding = PaddingMode.PKCS7;
            return sma;
        }

        /// <summary>
        ///     Generate a random Byte array of specified length
        /// </summary>
        /// <param name="count">Byte array length</param>
        /// <returns>random Byte array</returns>
        private static byte[] GenerateRandomBytes(int count)
        {
            var bytes = new byte[count];
            Rand.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        ///     Encrypted file
        /// </summary>
        /// <param name="inFile">file to be encrypted</param>
        /// <param name="outFile">Encrypted input file</param>
        /// <param name="password">encrypted password</param>
        public static void EncryptFile(string inFile, string outFile, string password)
        {
            using (FileStream fileIn = File.OpenRead(inFile), fileOut = File.OpenWrite(outFile))
            {
                long lSize = fileIn.Length; // input file length
                var bytes = new byte[BufferSize]; // cache

                // Get IV and salt
                byte[] iv = GenerateRandomBytes(16);
                byte[] salt = GenerateRandomBytes(16);

                // Create an encrypted object
                SymmetricAlgorithm sma = CreateRijndael(password, salt);
                sma.IV = iv;

                // Write IV and salt at the beginning of the output file
                fileOut.Write(iv, 0, iv.Length);
                fileOut.Write(salt, 0, salt.Length);

                // Create hash encryption
                HashAlgorithm hasher = SHA256.Create();
                using (CryptoStream cryptoStream =
                        new CryptoStream(fileOut, sma.CreateEncryptor(), CryptoStreamMode.Write),
                    cHash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
                {
                    var bw = new BinaryWriter(cryptoStream);
                    bw.Write(lSize);

                    bw.Write(FcTag);

                    // Read and write byte blocks to encrypted stream buffer
                    int read = -1; // Number of input files read
                    while ((read = fileIn.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        cryptoStream.Write(bytes, 0, read);
                        cHash.Write(bytes, 0, read);
                    }

                    // Close the encrypted stream
                    cHash.Flush();
                    cHash.Close();

                    // read the hash
                    byte[] hash = hasher.Hash;

                    // input file write hash
                    cryptoStream.Write(hash, 0, hash.Length);

                    // Close the file stream
                    cryptoStream.Flush();
                    cryptoStream.Close();
                }
            }
        }

        /// <summary>
        ///     decrypt the file
        /// </summary>
        /// <param name="inFile">file to be decrypted</param>
        /// <param name="outFile">output file after decryption</param>
        /// <param name="password">decryption password</param>
        public static void DecryptFile(string inFile, string outFile, string password)
        {
            // Create an open file stream
            using (FileStream fileIn = File.OpenRead(inFile), fileOut = File.OpenWrite(outFile))
            {
                var bytes = new byte[BufferSize];
                var outValue = 0;

                var iv = new byte[16];
                fileIn.Read(iv, 0, 16);
                var salt = new byte[16];
                fileIn.Read(salt, 0, 16);

                SymmetricAlgorithm sma = CreateRijndael(password, salt);
                sma.IV = iv;

                long lSize = -1;

                // Create a hash object, verify the file
                HashAlgorithm hasher = SHA256.Create();

                using (CryptoStream cryptoStream =
                        new CryptoStream(fileIn, sma.CreateDecryptor(), CryptoStreamMode.Read),
                    cHash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
                {
                    // read file length
                    var br = new BinaryReader(cryptoStream);
                    lSize = br.ReadInt64();
                    ulong tag = br.ReadUInt64();

                    if (FcTag != tag)
                        throw new CryptoHelpException("File destroyed");

                    long numReads = lSize / BufferSize;

                    long slack = lSize % BufferSize;

                    int read = -1;
                    for (var i = 0; i < numReads; ++i)
                    {
                        read = cryptoStream.Read(bytes, 0, bytes.Length);
                        fileOut.Write(bytes, 0, read);
                        cHash.Write(bytes, 0, read);
                        outValue += read;
                    }

                    if (slack > 0)
                    {
                        read = cryptoStream.Read(bytes, 0, (int) slack);
                        fileOut.Write(bytes, 0, read);
                        cHash.Write(bytes, 0, read);
                        outValue += read;
                    }

                    cHash.Flush();
                    cHash.Close();

                    fileOut.Flush();
                    fileOut.Close();

                    byte[] curHash = hasher.Hash;

                    // Get comparison and old hash object
                    var oldHash = new byte[hasher.HashSize / 8];
                    read = cryptoStream.Read(oldHash, 0, oldHash.Length);
                    if (oldHash.Length != read || !CheckByteArrays(oldHash, curHash))
                        throw new CryptoHelpException("File destroyed");
                }

                if (outValue != lSize)
                    throw new CryptoHelpException("File size mismatch");
            }
        }
    }
}