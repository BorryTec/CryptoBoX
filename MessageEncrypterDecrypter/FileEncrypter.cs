using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CryptoBoX
{
    class FileEncrypter
    {
        
        private string source;
        private string target;
        private byte[] key;
        private byte[] iV;
        BackgroundWorker worker;

        public FileEncrypter(string inputFile, string outputFile, byte[] inKey, byte[] inIV, bool encrypt)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException(string.Format(@"Source file was not found. FileName: {0}", source));

            source = inputFile;
            target = outputFile;
            key = inKey;
            iV = inIV;
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = false;
            worker.WorkerReportsProgress = true;
            if (encrypt)
                worker.DoWork += EncryptFile;
            else
                worker.DoWork += DecryptFile;
        }

        public void EncryptFile(object encrypt, DoWorkEventArgs e)
        {
            int bufferSize = 1024 * 512;
            using (FileStream inStream = new FileStream(source, FileMode.Open))
            using (FileStream outStream = new FileStream(target, FileMode.OpenOrCreate))
            using (RijndaelManaged rijAlg = new RijndaelManaged())

            {
                int bytesRead = -1;
                var totalReads = 0;
                var totalBytes = inStream.Length;
                byte[] bytes = new byte[bufferSize];
                int prevPercent = 0;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(key, iV);

                    using (CryptoStream csEncrypt = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
                    {
                        while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                        {
                            csEncrypt.Write(bytes, 0, bytesRead);
                            totalReads += bytesRead;
                            int percent = System.Convert.ToInt32(((decimal)totalReads / (decimal)totalBytes) * 100);
                            if (percent != prevPercent)
                            {
                                worker.ReportProgress(percent);
                                prevPercent = percent;
                            }
                        }
                    
                        
                }



                //byte[] fileBytes = FileToByteArray(inputFile);
                // byte[] crypByte = enc.EncryptBytes(fileBytes, key, iV);
                // File.WriteAllBytes(outputFile, fileBytes);
                //FileStream fsOut = new FileStream(outputFile, FileMode.Create);
                //fsOut.Write(crypByte,0,crypByte.Length);
                //fsOut.Close();
            }

        }

        public void DecryptFile(object sender, DoWorkEventArgs e)
        {
            int bufferSize = (1024 * 512);
            using (FileStream inStream = new FileStream(source, FileMode.Open))
            using (FileStream outStream = new FileStream(target, FileMode.Create))
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
                        csDecode.Write(bytes, 0, bytesRead);
                        totalReads += bytesRead;
                        int percent = System.Convert.ToInt32(((decimal)totalReads / (decimal)totalBytes) * 100);
                        if (percent != prevPercent)
                        {
                            worker.ReportProgress(percent);
                            prevPercent = percent;
                        }
                    }
                }

            }
        }

        //public byte[] FileToByteArray(string fileName)
        //{
        //    byte[] buff = null;
        //    FileStream fs = new FileStream(fileName,
        //                                   FileMode.Open,
        //                                   FileAccess.Read);
        //    BinaryReader br = new BinaryReader(fs);
        //    long numBytes = new FileInfo(fileName).Length;
        //    buff = br.ReadBytes((int)numBytes);
        //    return buff;
        //}
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

    }


}
