using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace RSAEncryptor
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        private X509Certificate2 crtCert = new X509Certificate2();

        public Form1()
        {
            InitializeComponent();
            openFileDialog1.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string inputText = InputTxtBox.Text;

                ///Reset the Encrypted Text Box
                EncryptedTxtBox.Text = string.Empty;

                //2048 bit rsa key pair
                var csp = new RSACryptoServiceProvider(2048);

                //Public key
                var pubKey = csp.ExportParameters(false);

                csp = (RSACryptoServiceProvider)crtCert.PublicKey.Key;
                pubKey = csp.ExportParameters(false);

                //Convert to bytes for encryption
                var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(inputText);

                byte[] bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

                //Convert to base 64 string 
                var cypherText = Convert.ToBase64String(bytesCypherText);

                EncryptedTxtBox.Text = cypherText;
            }
            catch(Exception ex)
            {
                log.Error(string.Format("button1_Click Error: {0}", ex.ToString()));
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string CRTFileName = openFileDialog1.FileName;

            try
            {
                if (!string.IsNullOrEmpty(CRTFileName))
                    crtCert = new X509Certificate2(CRTFileName);
            }catch(Exception ex)
            {
                log.Error(string.Format("openFileDialog Error: {0}", ex.ToString()));
            }
        }
    }
}
