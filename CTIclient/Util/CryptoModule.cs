/**
 * Crypto module. Mostly based on the information @:
 * http://stackoverflow.com/questions/11873878/c-sharp-encryption-to-php-decryption
 * http://msdn.microsoft.com/en-us/library/system.security.cryptography.rijndael(v=vs.110).aspx
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
    public class CryptoModule
    {
        private String sKy;
        private String sIV;
        private RijndaelManaged rijndaelModule;

        public CryptoModule(String sKy, String sIV)
        {
            this.sKy = sKy;
            this.sIV = sIV;
            this.rijndaelModule = createNewRijndael();
        }

        /**
         * Create new Rijndeal module
         * 
         * @return rijndeal module
         * 
         */
        private RijndaelManaged createNewRijndael()
        {
            // Create Rijndael once for performance reasons, also use 128-bit mode
            // since it is faster than 256-bit mode.
            return new RijndaelManaged()
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };
        }
        
        /**
         * Decrypt 128 Bit Base64 encoded String
         * 
         * @param sKy
         * @param sIV
         * @param prm_text_to_decrypt
         * @return plaintext
         * 
         */
        public String DecryptRJ128(String prm_text_to_decrypt)
        {
            // Get key and iv as byte array
            var key = Encoding.ASCII.GetBytes(sKy);
            var IV = Encoding.ASCII.GetBytes(sIV);

            // Create decryptor
            var decryptor = this.rijndaelModule.CreateDecryptor(key, IV);

            // Convert from base64 & create bytearray
            var sEncrypted = Convert.FromBase64String(prm_text_to_decrypt);
            var fromEncrypt = new byte[sEncrypted.Length];

            // Create memorystream from byte array, create cryptostream
            var msDecrypt = new MemoryStream(sEncrypted);
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

            // Decrypt cryptostream and turn byte array to normal string, then return result
            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
            return (Encoding.ASCII.GetString(fromEncrypt)).TrimEnd('\0');
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
        public String EncryptRJ128(string prm_text_to_encrypt)
        {
            // Get key and iv as byte array
            var key = Encoding.ASCII.GetBytes(sKy);
            var IV = Encoding.ASCII.GetBytes(sIV);

            // Create encryptor
            var encryptor = this.rijndaelModule.CreateEncryptor(key, IV);

            // Create memorystream and cryptostream
            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            // Get bytearray from input string
            var toEncrypt = Encoding.ASCII.GetBytes(prm_text_to_encrypt);
            
            // Write bytearray to cryptostream and memorystream
            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();
            
            // Write encrypted memorystream to bytearray and return as base64 String
            var encrypted = msEncrypt.ToArray();
            return (Convert.ToBase64String(encrypted));
        }
    }
}