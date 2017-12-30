using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace RSAEncryptionTest
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                //Create new 2048 bit rsa key pair
                var csp = new RSACryptoServiceProvider(2048);

                //Private key
                var privKey = csp.ExportParameters(true);

                //Public key ...
                var pubKey = csp.ExportParameters(false);

                string pubKeyString = Properties.Settings.Default.PublicKeyString;
                string privKeyString = Properties.Settings.Default.PrivateKeyString;

                var privatePem = Properties.Settings.Default.PrivatePFXFile;
                X509Certificate2 privateCert = new X509Certificate2(privatePem, "testpassword", X509KeyStorageFlags.Exportable);

                var publicPem = Properties.Settings.Default.PublicCRTFile;

                string publicCRTString = Properties.Settings.Default.PublicCRTString;
                byte[] binaryCRT = Convert.FromBase64String(publicCRTString);
                X509Certificate2 publicCert = new X509Certificate2(binaryCRT);

                csp = (RSACryptoServiceProvider)publicCert.PublicKey.Key;
                pubKey = csp.ExportParameters(false);

                log.Debug(string.Format("Public Key: {0}", csp.ToXmlString(false)));

                var plainTextData = "foobar";

                var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

                log.Debug(string.Format("Plain Text Data to Encrypt: {0}", plainTextData));
                
                var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

                var cypherText = Convert.ToBase64String(bytesCypherText);

                log.Debug(string.Format("Encrypted Text: {0}", cypherText));

                bytesCypherText = Convert.FromBase64String(cypherText);

                csp = (RSACryptoServiceProvider)privateCert.PrivateKey;
                privKey = csp.ExportParameters(true);

                log.Debug(string.Format("Private Key: {0}", csp.ToXmlString(true)));

                bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

                plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

                log.Debug(string.Format("Plain Text Data Decrypted using Private Key: {0}", plainTextData));
            }catch(Exception e)
            {
                log.Error(string.Format("Error in RSA Encryption/Decryption: {0}", e.ToString()));

            }
        }

        /// <summary>
        /// Converts PEM to bytes for import. Not used
        /// </summary>
        /// <param name="pemString"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        static byte[] GetBytesFromPEM(string pemString, string section)
        {
            var header = String.Format("-----BEGIN {0}-----", section);
            var footer = String.Format("-----END {0}-----", section);

            var start = pemString.IndexOf(header, StringComparison.Ordinal) + header.Length;
            var end = pemString.IndexOf(footer, start, StringComparison.Ordinal);
            string result = pemString.Substring(start, (end - start));
            result = result.TrimStart(Environment.NewLine.ToCharArray());
            result = result.TrimEnd(Environment.NewLine.ToCharArray());

            if (start < 0 || end < 0)
            {
                return null;
            }

            return Convert.FromBase64String(result);
        }
    }
}
