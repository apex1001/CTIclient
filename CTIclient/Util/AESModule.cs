/**
 * Crypto module. Borrowed from:
 * http://stackoverflow.com/questions/11873878/c-sharp-encryption-to-php-decryption
 *
 * Edit: Switched to 128 bit
 *  
 * @author V. Vogelesang
 * 
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace CTIclient
{
    /**
     * CryptoModule class
     * 
     * Encrypt/decrypt 128 bit AES Base64
     * 
     */
    public static class AESModule
    {

        /**
         * Decrypt 128 Bit Base64 encoded String
         * 
         * @param sKy
         * @param sIV
         * @param prm_text_to_decrypt
         * @return plaintext
         * 
         */
        public static string DecryptRJ128(string sKy, string sIV, string prm_text_to_decrypt)
        {
            var sEncryptedString = prm_text_to_decrypt;
            var myRijndael = new RijndaelManaged()
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };

            var key = Encoding.ASCII.GetBytes(sKy);
            var IV = Encoding.ASCII.GetBytes(sIV);
            var decryptor = myRijndael.CreateDecryptor(key, IV);
            var sEncrypted = Convert.FromBase64String(sEncryptedString);
            var fromEncrypt = new byte[sEncrypted.Length];
            var msDecrypt = new MemoryStream(sEncrypted);
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

            return (Encoding.ASCII.GetString(fromEncrypt));
        }

        /**
         * Encrypt string to 128 Bit Base64
         *
         * @param sKy
         * @param sIV
         * @param prm_text_to_encrypt
         * @return ciphertext
         *
         */
        public static string EncryptRJ128(string sKy, string sIV, string prm_text_to_encrypt)
        {
            var sToEncrypt = prm_text_to_encrypt;
            var myRijndael = new RijndaelManaged()
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };

            var key = Encoding.ASCII.GetBytes(sKy);
            var IV = Encoding.ASCII.GetBytes(sIV);
            var encryptor = myRijndael.CreateEncryptor(key, IV);
            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            var toEncrypt = Encoding.ASCII.GetBytes(sToEncrypt);
            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();
            var encrypted = msEncrypt.ToArray();

            return (Convert.ToBase64String(encrypted));
        }
    }
}