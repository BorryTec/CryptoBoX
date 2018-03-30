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
        string currentProcess;
        string tmp;
        bool encrypt;
        bool wipeOnClose;


        #region WipeFileEventFields
        string fName;
        int cPass;
        int tPass;
        int fSectors;
        int fNum;
        int fArrayNum; 
        #endregion


        public FileEncrypterForm()
        {
            InitializeComponent();
            button1.Text = "Encrypt";
            decrypt_Button.Text = "Decrypt";
            label1.Text = "";
            password = "";
            wipeOnClose = false;
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
                            compress.PropertyChanged += PropertyChanged;                         
                            compress.StartCompression();
                            encrypt = true;
                        }
                    }
                }
            }
        }

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           currentProcess = e.PropertyName;
        }

        private void CompressCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (encrypt)
            {
                progressBar1.Value = 0;
                FileEncrypter copy = new FileEncrypter(folderPath + "\\comp", Path.Combine(folderPath, saveName), loadedKey, loadedIV, true);
                copy.Completed += EncryptCompleted;
                copy.PropertyChanged += PropertyChanged;
                copy.ProgressChanged += CopyProgressChanged;
                copy.StartEncrypt();
                //label1.Text = "Encrypting Archive";
            }
            if (!encrypt)
            {
                WipeFile wipe = new WipeFile();
                wipe.FileStatusEvent += Wipe_FileStatusEvent;
                wipe.PassInfoEvent += Wipe_PassInfoEvent;
                wipe.SectorInfoEvent += Wipe_SectorInfoEvent;
                wipe.SecureDelete(folderPath + "\\file.dec",2);
                File.Delete(folderPath + "\\file.dec");
                progressBar1.Value = 0;
                label1.Text = "";
                MessageBox.Show("Finished", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            WipeFile wipe = new WipeFile();
            //wipe.SecureDelete(folderPath + "\\comp", 1);
           // wipe.WipeErrorEvent += Wipe_WipeErrorEvent;
           // wipe.PassInfoEvent += Wipe_PassInfoEvent;
            //wipe.SectorInfoEvent += Wipe_SectorInfoEvent;

            //File.Delete(folderPath + " \\comp");
            this.BringToFront();
                progressBar1.Value = 0;
                currentProcess = "";
                MessageBox.Show("Finished", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (!encrypt)
            {
                CompressFolder compress = new CompressFolder(filePaths, folderPath, CompressionOption.Decompress);
                compress.ProgressChanged += CopyProgressChanged;
                compress.PropertyChanged += PropertyChanged;
                compress.Completed += CompressCompleted;
                compress.StartCompression();
                //label1.Text = "Unpacking";
            }
            // do something
        }

        private void CopyProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            label1.Text = currentProcess;
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
                            copy.PropertyChanged += PropertyChanged;
                            copy.StartEncrypt();
                            label1.Text = "Decrypting Archive";
                        }
                    }
                }
            }
        }
        private void FileEncrypterForm_Load(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            wipeOnClose = checkBox1.Checked;
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void FileEncrypterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(wipeOnClose)
            {
                currentProcess = "Wiping Decrypred Files";
                string[] lastDecryptedBatch = Directory.GetFiles(folderPath);
                WipeFile wipe = new WipeFile();
                wipe.PassInfoEvent += Wipe_PassInfoEvent;
                wipe.FileStatusEvent += Wipe_FileStatusEvent;
                wipe.SectorInfoEvent += Wipe_SectorInfoEvent;
                wipe.WipeDoneEvent += Wipe_WipeDoneEvent;
                wipe.SecureDelete(lastDecryptedBatch, 3);
            }

        }

        private void Wipe_WipeDoneEvent(WipeDoneEventArgs e)
        {
            Application.Exit();
        }

        private void Wipe_FileStatusEvent(FileStatusEventArgs e)
        {
            fName = e.FileName;
            fNum = e.FileArrayPos;
            fArrayNum = e.FileAraySize;
        }

        private void Wipe_PassInfoEvent(PassInfoEventArgs e)
        {
            cPass = e.CurrentPass;
            tPass = e.TotalPasses;
            currentProcess = "Deleting File " + fNum.ToString() + " of " + fArrayNum.ToString() + " pass " + cPass.ToString() + " of " + tPass.ToString();
            label1.Text = currentProcess;
            label1.Refresh();
        }

        private void Wipe_SectorInfoEvent(SectorInfoEventArgs e)
        {
            int percent = System.Convert.ToInt32(((decimal)e.CurrentSector / (decimal)e.TotalSectors) * 100);
            progressBar1.Value = percent;
        }

        private void label1_TextChanged(object sender, EventArgs e)
        {
           
        }
    }
}
