using System;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Text = "File Encrypter";
            button2.Text = "Text Encrypter";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileEncrypterForm enc = new FileEncrypterForm();
            enc.Show();
            enc.BringToFront();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CryptoBoX cb = new CryptoBoX();
            cb.Show();
            cb.BringToFront();
            this.Hide();
        }
    }
}
