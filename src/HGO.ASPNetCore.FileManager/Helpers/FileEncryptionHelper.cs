using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HGO.ASPNetCore.FileManager.Helpers
{
    public class FileEncryptionHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _magicNumber; // Magic number for encrypted files
        private readonly bool _useEncryption;

        public FileEncryptionHelper(string encryptionKey, bool useEncryption, string magicNumber = "ENCFILE")
        {
            _useEncryption = useEncryption;
            _magicNumber = Encoding.UTF8.GetBytes(magicNumber);
            _key = GenerateKeyFromPassword(encryptionKey);
        }

        private byte[] GenerateKeyFromPassword(string password)
        {
            if (!_useEncryption)
                return [];

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Encrypts the input stream and returns a new encrypted stream with IV and magic number prepended
        public Stream EncryptStream(Stream inputStream)
        {
            if (!_useEncryption)
                return inputStream;

            var outputStream = new MemoryStream();

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();

                // Write magic number and IV to the beginning of the output stream
                outputStream.Write(_magicNumber, 0, _magicNumber.Length);
                outputStream.Write(aes.IV, 0, aes.IV.Length);

                using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
                {
                    inputStream.CopyTo(cryptoStream);
                    cryptoStream.FlushFinalBlock(); // Ensure all data is written
                }

                outputStream.Position = 0; // Reset position for reading
                return outputStream;
            }
        }

        // Decrypts the input stream if encrypted, else returns the original stream
        public Stream DecryptStream(Stream inputStream)
        {
            if (!_useEncryption && !IsEncrypted(inputStream))
                return inputStream;

            // Check for magic number to determine if the file is encrypted
            if (_useEncryption && !IsEncrypted(inputStream))
            {
                // Return a stream with an error message
                var fileStream = new MemoryStream();
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write("This file is encrypted but there is no decryption key provided.");
                    writer.Flush();
                }
                fileStream.Position = 0; // Reset stream for reading
                return fileStream;
            }

            var outputStream = new MemoryStream();

            using (var aes = Aes.Create())
            {
                aes.Key = _key;

                // Read IV from the input stream (immediately after the magic number)
                byte[] iv = new byte[aes.BlockSize / 8];
                if (inputStream.Read(iv, 0, iv.Length) < iv.Length)
                {
                    throw new ArgumentException("Invalid input stream. IV is missing or corrupted.");
                }

                aes.IV = iv;

                // Decrypt and copy the input stream to the output stream
                using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream, Encoding.UTF8))
                using (var writer = new StreamWriter(outputStream, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            writer.WriteLine(line);
                        }
                    }
                    writer.Flush();
                }
            }

            outputStream.Position = 0; // Reset position for reading
            return outputStream;
        }

        public bool IsEncrypted(Stream inputStream)
        {
            // Check for magic number to determine if the file is encrypted
            byte[] fileMagicNumber = new byte[_magicNumber.Length];
            return inputStream.Read(fileMagicNumber, 0, fileMagicNumber.Length) == _magicNumber.Length && fileMagicNumber.SequenceEqual(_magicNumber);
        }
    }
}
