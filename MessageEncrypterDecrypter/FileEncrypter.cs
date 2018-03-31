using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;

namespace CryptoBoX
{
    class FileEncrypter
    {
        private string source;
        private string target;
        private byte[] key;
        private byte[] iV;
        BackgroundWorker worker;
        private string _methodRunning;
        private CancellationToken _cancelToken;
        private bool canceled;
        public event PropertyChangedEventHandler PropertyChanged;

        public string AValue
        {
            get
            {
                return _methodRunning;
            }
            set
            {
                if (value != _methodRunning)
                {
                    _methodRunning = value;
                    OnPropertyChanged(AValue);
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public FileEncrypter(string inputFile, string outputFile, byte[] inKey, byte[] inIV, bool encrypt, CancellationToken cancelToken)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException(string.Format(@"Source file was not found. FileName: {0}", source));

            source = inputFile;
            target = outputFile;
            key = inKey;
            iV = inIV;
            worker = new BackgroundWorker();
            _cancelToken = cancelToken;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            if (encrypt)
                worker.DoWork += EncryptFile;
            else
                worker.DoWork += DecryptFile;
        }

        void CancelOperations(string target, string filter)
        {
            WipeFile wipe = new WipeFile();
            wipe.WipeDoneEvent += Wipe_WipeDoneEvent;
            AValue = "Cleaning";
            string dir = Path.GetDirectoryName(target);
            string[] dirFiles = Directory.GetFiles(dir);
            wipe.SecureDelete(dirFiles, 1, filter);
        }



        public void EncryptFile(object encrypt, DoWorkEventArgs e)
        {
           

            int bufferSize = 1024 * 512;
            using (FileStream inStream = new FileStream(source, FileMode.Open))
            using (FileStream outStream = new FileStream(target, FileMode.OpenOrCreate))
            using (RijndaelManaged rijAlg = new RijndaelManaged())

            {
                var bytesRead = -1;
                var totalReads = 0;
                var totalBytes = inStream.Length;
                byte[] bytes = new byte[bufferSize];
                int prevPercent = 0;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(key, iV);

                using (CryptoStream csEncrypt = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
                {
                    while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                    {
                        if (_cancelToken.IsCancellationRequested)
                        {
                            canceled = true;
                            csEncrypt.Dispose();
                            outStream.Dispose();
                            inStream.Dispose();
                            CancelOperations(target, ".none");
                            worker.Dispose();
                            return;
                        }

                        csEncrypt.Write(bytes, 0, bytesRead);
                        totalReads += bytesRead;
                        int percent = System.Convert.ToInt32(((decimal)totalReads / (decimal)totalBytes) * 100);
                        int processed = Convert.ToInt32(((long)totalReads / 1024) / 1024);
                        if (percent != prevPercent)
                        {
                            AValue = "Encrypting " + BytesToMB(processed);
                            worker.ReportProgress(percent);
                            prevPercent = percent;
                        }
                    }
                }
                outStream.Dispose();
                inStream.Dispose();
            }
            


        }

        public void DecryptFile(object sender, DoWorkEventArgs e)
        {
            try
            {
                int bufferSize = 1024 * 512;
                using (FileStream inStream = new FileStream(source, FileMode.Open))
                using (FileStream outStream = new FileStream(target+"\\file.dec", FileMode.Create))
                using (RijndaelManaged rijAlg = new RijndaelManaged())

                {
                    int bytesRead = -1;
                    var totalReads = 0;
                    var totalBytes = inStream.Length;
                    byte[] bytes = new byte[bufferSize];
                    int prevPercent = 0;

                    ICryptoTransform encryptor = rijAlg.CreateDecryptor(key, iV);

                    using (CryptoStream csDecode = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
                    {
                        while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                        {
                            if (_cancelToken.IsCancellationRequested)
                            {
                                canceled = true;
                                inStream.Dispose();
                                outStream.Dispose();
                                //csDecode.Dispose();
                                CancelOperations(target, ".enc");
                                return;
                            }
                            csDecode.Write(bytes, 0, bytesRead);
                            totalReads += bytesRead;
                            int percent = System.Convert.ToInt32(((decimal)totalReads / (decimal)totalBytes) * 100);
                            int processed = Convert.ToInt32(((long)totalReads / 1024) / 1024);
                            int gb = Convert.ToInt32(((long)totalBytes / 1024) / 1024);

                            if (percent != prevPercent)
                            {
                                AValue = "Decrypting " + BytesToMB(processed) + "/" + BytesToMB(gb);
                                worker.ReportProgress(percent);
                                prevPercent = percent;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Wipe_SectorInfoEvent(SectorInfoEventArgs e)
        {
            int percent = System.Convert.ToInt32(((decimal)e.CurrentSector / (decimal)e.TotalSectors) * 100);
            worker.ReportProgress(percent);
        }

        private void Wipe_PassInfoEvent(PassInfoEventArgs e)
        {
        }
        private void Wipe_WipeDoneEvent(WipeDoneEventArgs e)
        {
            if (canceled) { Application.Exit(); }
        }

        private void Wipe_WipeErrorEvent(WipeErrorEventArgs e)
        {
            MessageBox.Show("Error" + e);
        }

        public event ProgressChangedEventHandler ProgressChanged
        {
            add { worker.ProgressChanged += value; }
            remove { worker.ProgressChanged -= value; }
        }

        public event RunWorkerCompletedEventHandler Completed
        {
            add { worker.RunWorkerCompleted += value; }
            remove { worker.RunWorkerCompleted -= value; }
        }

        public void StartEncrypt()
        {
            worker.RunWorkerAsync();
        }

        string BytesToMB(int processed)
        {
            decimal tmp;
            tmp = (decimal)processed / 1024;
            if (processed >= 0 && processed < 1024)
                return processed.ToString() + "MB";
            else return String.Format("{0:0.00} GB", tmp);
        }

    }


}
