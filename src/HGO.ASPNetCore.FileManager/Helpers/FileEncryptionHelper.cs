using System;
using System.IO;
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
                return Array.Empty<byte>();

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
            var outputStream = new MemoryStream();

            // Check if no encryption is applied
            if (!_useEncryption || !IsEncrypted(inputStream))
            {
                // If no encryption is used, return the input stream as is
                inputStream.Position = 0; // Reset the position to the start of the stream
                return inputStream;  // Return the original input stream
            }

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
                {
                    try
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                    catch (CryptographicException)
                    {
                        return GetErrorStream(); // Return a stream containing the error message
                    }
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

        // Method to save the stream to a file
        public void SaveStreamToFile(Stream encryptedStream, string filePath)
        {
            // Ensure the file is not being used by another process
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encryptedStream.CopyTo(fileStream);
            }
        }

        // Example usage for encrypting and saving a file
        public void EncryptAndSaveFile(string inputFilePath, string outputFilePath)
        {
            using (var inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                // Encrypt the file
                var encryptedStream = EncryptStream(inputFileStream);

                // Save the encrypted stream to a file
                SaveStreamToFile(encryptedStream, outputFilePath);
            }
        }

        // Example usage for decrypting and saving a file
        public void DecryptAndSaveFile(string inputFilePath, string outputFilePath)
        {
            using (var inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                // Decrypt the file
                var decryptedStream = DecryptStream(inputFileStream);

                // Save the decrypted stream to a file
                SaveStreamToFile(decryptedStream, outputFilePath);
            }
        }

        private Stream GetErrorStream()
        {
            var errorMessage = "Decryption failed: Invalid key or corrupted data.";
            var errorStream = new MemoryStream();
            var writer = new StreamWriter(errorStream);
            writer.Write(errorMessage);
            writer.Flush();
            errorStream.Position = 0; // Reset position for reading
            return errorStream;
        }
    }
}
