using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace RSAKeyGenerator
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

                string pubKeyString;
                {
                    var sw = new System.IO.StringWriter();
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    xs.Serialize(sw, pubKey);
                    pubKeyString = sw.ToString();
                    log.Debug("Public Key Generated");
                }

                string privKeyString;
                {
                    var sw = new System.IO.StringWriter();
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    xs.Serialize(sw, privKey);
                    privKeyString = sw.ToString();
                    log.Debug("Private Key Generated");
                }

                //Load public key
                csp = new RSACryptoServiceProvider();
                csp.ImportParameters(pubKey);

                //Encrypytion data
                string plainTextData = "Encryption Test";
                string decryptedTextData = string.Empty;

                //Encryption data in bytes
                var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

                var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

                //Base 64 string
                var cypherText = Convert.ToBase64String(bytesCypherText);

                bytesCypherText = Convert.FromBase64String(cypherText);

                //To decrypt, create CSP and load private key
                csp = new RSACryptoServiceProvider();
                csp.ImportParameters(privKey);

                //decrypt and strip pkcs#1.5 padding
                bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

                //Retrieve original text
                decryptedTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

                if (plainTextData.Equals(decryptedTextData))
                {
                    log.Debug(string.Format("Success decrypted text matched original text data - Original: {0}, Decrypted: {1}", plainTextData, decryptedTextData));

                    ///Export public key to file for use
                    using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.RSAPublicKeyFileName))
                    {
                        sw.Write(pubKeyString);
                        log.Debug("Finished Exporting Public Key to File");
                    }

                    ///Export private key to file for use
                    using (StreamWriter sw = new StreamWriter(Properties.Settings.Default.RSAPrivateKeyFileName))
                    {
                        sw.Write(privKeyString);
                        log.Debug("Finished Exporting Private Key to File");
                    }
                }
                else
                {
                    log.Error(string.Format("Decrypted text did not match original text data - Original: {0}, Decrypted: {1}", plainTextData, decryptedTextData));
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error in RSA Key Generator: {0}", e.ToString()));
            }
        }
    }
}
