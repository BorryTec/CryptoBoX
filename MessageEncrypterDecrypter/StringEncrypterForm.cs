using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoBoX
{
    public partial class CryptoBoX : Form
    {
        byte[] loadedKey;
        byte[] loadedIV;
        string textbox;
        string password;

        public CryptoBoX()
        {

            InitializeComponent();
            textbox = textBox2.Text;
            //loadedKey = File.ReadAllBytes("encrypt2.key");
            //loadedIV = File.ReadAllBytes("encrypt2.iv");
            //File.WriteAllBytes("encrypt2.key",loadedKey);
            //File.WriteAllBytes("encrypt2.iv", loadedIV);

            button1.Text = "Decrypt";
            button3.Text = "Encrypt";
            password_label1.Text = "Password";
            password_label1.TextAlign = ContentAlignment.MiddleCenter;

        }


        private void button3_Click(object sender, EventArgs e)
        {
            Encrypter encrpter = new Encrypter();
            if (textbox == null || textbox.Length <= 0)
            {
                MessageBox.Show("No Text To Encrypt", "Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(loadedKey == null)
            {
                MessageBox.Show("Enter Password", "Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] temp = encrpter.EncryptStringToBytes(textbox, loadedKey, loadedIV);
            textBox2.Text = Convert.ToBase64String(temp);
            textBox2.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Encrypter encrpter = new Encrypter();
            if (textbox == null || textbox.Length <= 0)
            {
                MessageBox.Show("No Text To Decrypt", "Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] bytem = null;
            try
            {
                bytem = Convert.FromBase64String(textbox);
            }
            catch
            {
                return;
            }

            string temp = "";
            try
            {
                temp = encrpter.DecryptStringFromBytes(bytem, loadedKey, loadedIV);
            }
            catch 
            {
                MessageBox.Show("Wrong Password","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                temp = textbox;
            }
            textBox2.Text = temp;
            textBox2.Update();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            EncryptionKeyGenerator newKey = new EncryptionKeyGenerator();
            
                password = textBox1.Text;
                newKey.GenerateKey(password, out loadedKey, out loadedIV);
            
        }

  

       
        private static string RandomHexString()
        {
            // 64 character precision or 256-bits
            Random rdm = new Random();
            string hexValue = string.Empty;
            int num;

            for (int i = 0; i < 8; i++)
            {
                num = rdm.Next(0, int.MaxValue);
                hexValue += num.ToString("X8");
            }

            return hexValue;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = RandomHexString();
        }

        private void CryptoBoX_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FileEncrypterForm fef = new FileEncrypterForm();
            fef.Show();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textbox = textBox2.Text;
        }

        private void CryptoBoX_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}



