using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoBoX
{
    public partial class FileEncrypterForm : Form
    {
        byte[] loadedKey;
        byte[] loadedIV;
        string textbox;
        string password;
        string filePath;
        string[] filePaths;
        string folderPath;
        string saveName;
        bool encrypt;

        public FileEncrypterForm()
        {
            InitializeComponent();
            button1.Text = "Encrypt";
            decrypt_Button.Text = "Decrypt";
            label1.Text = "";
            password = "";
        }
        //Compression and Encryption
        private void button1_Click(object sender, EventArgs e)
        {
            if (password == null | password.Length <= 0)
            {
                MessageBox.Show("No Password Entered");
                return;
            }
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    openDialog.Multiselect = true;
                    openDialog.Title = "Choose Files";
                    saveDialog.Filter = "Encoding | *.enc";
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        saveDialog.Title = "Save Encrypted File To";
                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            saveName = saveDialog.FileName;
                            folderPath = Path.GetDirectoryName(saveName);
                            filePaths = openDialog.FileNames;
                            //File.Create(saveName);
                            CompressFolder compress = new CompressFolder(filePaths, folderPath + "\\comp", CompressionOption.Compress);
                            compress.ProgressChanged += CopyProgressChanged;
                            compress.Completed += CompressCompleted;
                            compress.StartCompression();
                            encrypt = true;
                            label1.Text = "Creating Archive";
                        }
                    }
                }
            }
        }

        private void CompressCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (encrypt)
            {
                progressBar1.Value = 0;
                FileEncrypter copy = new FileEncrypter(folderPath + "\\comp", Path.Combine(folderPath, saveName), loadedKey, loadedIV, true);
                copy.Completed += EncryptCompleted;
                copy.ProgressChanged += CopyProgressChanged;
                copy.StartEncrypt();
                label1.Text = "Encrypting Archive";
            }
            if (!encrypt)
            {
                File.Delete(folderPath + "\\file.dec");
                progressBar1.Value = 0;
                label1.Text = "";
                MessageBox.Show("Finished");

            }
        }
        /********************************************/


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            EncryptionKeyGenerator newKey = new EncryptionKeyGenerator();              
                    password = textBox1.Text;
                    newKey.GenerateKey(password, out loadedKey, out loadedIV);                      
        }



        private void EncryptCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            worker.Dispose();
            if (encrypt)
            {

                File.Delete(folderPath + " \\comp");
                this.BringToFront();
                progressBar1.Value = 0;
                label1.Text = "";
                MessageBox.Show("Finished", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            if (!encrypt)
            {
                using (CompressFolder compress = new CompressFolder(filePaths, folderPath, CompressionOption.Decompress))
                {
                    compress.ProgressChanged += CopyProgressChanged;
                    compress.Completed += CompressCompleted;
                    compress.StartCompression();
                    label1.Text = "Unpacking Archive";
                }
            }
            // do something
        }

        private void CopyProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // change progress bar or whatever
            if (e.ProgressPercentage >= 0)
                progressBar1.Value = e.ProgressPercentage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                filePaths = openDialog.FileNames;
            }
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            if (password == null | password.Length <= 0)
            {
                MessageBox.Show("No Password Entered");
                return;
            }
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    openDialog.Filter = "Encrypted Files|*.enc";
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        folderDialog.Description = "Choose Folder To Decrypt Files To";
                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            encrypt = false;
                            folderPath = folderDialog.SelectedPath;

                            filePaths = openDialog.FileNames;
                            FileEncrypter copy = new FileEncrypter(filePaths[0], folderPath + "\\file.dec", loadedKey, loadedIV, encrypt);
                            copy.Completed += EncryptCompleted;
                            copy.ProgressChanged += CopyProgressChanged;
                            copy.StartEncrypt();
                            label1.Text = "Decrypting Archive";
                        }

                    }
                }

            }




        }
    }
}
