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
            inputStream.Position = 0; // Reset the position to the start of the stream
            var outputStream = new MemoryStream();

            // Check if no encryption is applied
            if (!_useEncryption || IsEncrypted(inputStream))
            {
                inputStream.CopyTo(outputStream);
                return outputStream;
            }


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
                    cryptoStream.FlushFinalBlock();
                }
            }

            outputStream.Position = 0; // Reset position for reading
            return outputStream;
        }

        // Decrypts the input stream if encrypted, else returns the original stream
        public Stream DecryptStream(Stream inputStream)
        {
            inputStream.Position = 0; // Reset the position to the start of the stream

            var outputStream = new MemoryStream();
            // Check if no encryption is applied
            if (!_useEncryption || !IsEncrypted(inputStream))
            {
                inputStream.CopyTo(outputStream);
                return outputStream;
            }

            using (var aes = Aes.Create())
            {
                aes.Key = _key;

                // Move the input stream position after reading the magic number
                inputStream.Position = _magicNumber.Length;

                // Read the IV from the input stream
                byte[] iv = new byte[aes.BlockSize / 8];
                if (inputStream.Read(iv, 0, iv.Length) < iv.Length)
                    return GetErrorStream();

                aes.IV = iv;

                using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    try
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                    catch (CryptographicException)
                    {
                        return GetErrorStream(); // Return error stream on decryption failure
                    }
                }
            }

            outputStream.Position = 0; // Reset position for reading
            return outputStream;
        }

        public bool IsEncrypted(Stream inputStream)
        {
            inputStream.Position = 0; // Reset to the beginning to check for magic number
            byte[] fileMagicNumber = new byte[_magicNumber.Length];
            bool isEncrypted = inputStream.Read(fileMagicNumber, 0, fileMagicNumber.Length) == _magicNumber.Length
                               && fileMagicNumber.SequenceEqual(_magicNumber);
            inputStream.Position = 0; // Reset the position to the start after checking
            return isEncrypted;
        }

        // Method to save the stream to a file
        public void SaveStreamToFile(Stream encryptedStream, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encryptedStream.CopyTo(fileStream);
            }
        }

        private Stream GetErrorStream()
        {
            var errorMessage = "Decryption failed: Invalid key or corrupted data.";
            var errorStream = new MemoryStream();
            using (var writer = new StreamWriter(errorStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(errorMessage);
                writer.Flush();
            }
            errorStream.Position = 0; // Reset position for reading
            return errorStream;
        }
    }
}
