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
        bool processRunning;


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
                            processRunning = true;
                        }
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            EncryptionKeyGenerator newKey = new EncryptionKeyGenerator();
            password = textBox1.Text;
            newKey.GenerateKey(password, out loadedKey, out loadedIV);
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
                            if (GetDriveFreeSpace(folderPath) > GetFileSize(filePaths))
                            {
                                FileEncrypter copy = new FileEncrypter(filePaths[0], folderPath + "\\file.dec", loadedKey, loadedIV, encrypt);
                                copy.Completed += EncryptCompleted;
                                copy.ProgressChanged += CopyProgressChanged;
                                copy.PropertyChanged += PropertyChanged;
                                copy.StartEncrypt();
                                label1.Text = "Decrypting Archive";
                                processRunning = true;
                            }
                            else
                            {
                                MessageBox.Show("Not Enough Space To Decrypt", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
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
            if (wipeOnClose)
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
            if(processRunning && !wipeOnClose)
            {
                if(MessageBox.Show("Still Process, Are You Sure You Want To Quit?","Warning",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.OK)
                {              
                    string[] dirFiles = Directory.GetFiles(folderPath);                   
                    WipeFile wipe = new WipeFile();
                    wipe.SectorInfoEvent += Wipe_SectorInfoEvent;
                    wipe.PassInfoEvent += Wipe_PassInfoEvent;
                    wipe.SecureDelete(dirFiles,1);
                }
            }

            Application.Exit();
        }

    

        private void label1_TextChanged(object sender, EventArgs e)
        {

        }

        #region WorkerEvents

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
                wipe.SecureDelete(folderPath + "\\file.dec", 2);
                File.Delete(folderPath + "\\file.dec");
                progressBar1.Value = 0;
                label1.Text = "";
                processRunning = false;
                MessageBox.Show("Finished", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void EncryptCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.Dispose();
            if (encrypt)
            {
                this.BringToFront();
                progressBar1.Value = 0;
                currentProcess = "";
                processRunning = false;
                MessageBox.Show("Finished", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (!encrypt)
            {
                CompressFolder compress = new CompressFolder(filePaths, folderPath, CompressionOption.Decompress);
                compress.ProgressChanged += CopyProgressChanged;
                compress.PropertyChanged += PropertyChanged;
                compress.Completed += CompressCompleted;
                compress.StartCompression();
            }
        }

        private void CopyProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            label1.Text = currentProcess;
            // change progress bar or whatever
            if (e.ProgressPercentage >= 0)
                progressBar1.Value = e.ProgressPercentage;
        } 

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            currentProcess = e.PropertyName;
        }
        #endregion

        #region FileWipeEvents
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
        #endregion

        #region Calculations
        long GetFileSize(string[] files)
        {
            long totalSize = -1;
            FileInfo tmp;
            foreach (var item in files)
            {
                tmp = new FileInfo(item);
                totalSize += tmp.Length;
            }
            return totalSize;
        }

        private long GetDriveFreeSpace(string path)
        {
            string tmp = Path.GetPathRoot(path);
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == tmp)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        } 
        #endregion

    }
}
