using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CryptoBoX
{
    public enum CompressionOption
    {
        Compress,
        Decompress,
    }

    class CompressFolder
    {
        private string[] _inPaths;
        private string _outPath;
        private string _methodRunning;
        BackgroundWorker _compWorker;
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

        public CompressFolder(string[] inPaths, string outPath, CompressionOption compressionOption)
        {
            _inPaths = inPaths;
            _outPath = outPath;
            _compWorker = new BackgroundWorker();
           
            _compWorker.WorkerSupportsCancellation = false;
            _compWorker.WorkerReportsProgress = true;

            if (compressionOption == CompressionOption.Compress)
            {
                _compWorker.DoWork += StartCompression;
            }
            if (compressionOption == CompressionOption.Decompress)
            {
                _compWorker.DoWork += StartDecompression;
            }
        }
   

        public void StartCompression(object sender, DoWorkEventArgs e)
        {
            ArchiveFiles();
        }
  

        void ArchiveFiles()
        {
            var zip = ZipFile.Open(_outPath + ".arc", ZipArchiveMode.Create);

            var fileCount = _inPaths.Length;
            var processed = 0;
            int prevPercent = -1;
            foreach (var item in _inPaths)
            {
                string fileName = Path.GetFileName(item);
                zip.CreateEntryFromFile(item, fileName);
                processed++;
                int percent = System.Convert.ToInt32(((decimal)processed / (decimal)fileCount) * 100);
                if (percent != prevPercent)
                {
                    AValue = "Archiving " +processed.ToString() + " of " + fileCount.ToString();                
                    _compWorker.ReportProgress(percent);
                    prevPercent = percent;
                }
            }
            zip.Dispose();
            CompressArchive();
        }

        void CompressArchive()
        {
            int bufferSize = 1024 * 512;
            using (FileStream fsInStream = File.Open(_outPath + ".arc", FileMode.Open))
            using (FileStream fsOutStream = new FileStream(_outPath, FileMode.Create))
            {
                int bytesRead = -1;
                var totalReads = 0;
                var totalBytes = fsInStream.Length;
                byte[] bytes = new byte[bufferSize];
                int prevPercent = 0;
                using (DeflateStream dfStream = new DeflateStream(fsOutStream, CompressionLevel.Optimal))
                {
                    while ((bytesRead = fsInStream.Read(bytes, 0, bufferSize)) > 0)
                    {
                        dfStream.Write(bytes, 0, bytesRead);
                        //csEncrypt.Write(bytes, 0, bytesRead);
                        totalReads += bytesRead;
                        int percent = System.Convert.ToInt32(((decimal)totalReads / (decimal)totalBytes) * 100);
                        if (percent != prevPercent)
                        {
                            AValue = "Compressing Archive";
                            _compWorker.ReportProgress(percent);
                            prevPercent = percent;
                        }
                    }
                }
            }
            WipeFile wipe = new WipeFile();
            AValue = "Cleaning";
            wipe.SecureDelete(_outPath + ".arc",3);
        }

  

        public void StartDecompression(object sender, DoWorkEventArgs e)
        {

            Directory.CreateDirectory(_outPath);
            int bufferSize = 1024 * 512;
            string outDir = Path.GetDirectoryName(_outPath);
            using (FileStream fsInStream = File.Open(_outPath+"\\file.dec", FileMode.Open))
            using (FileStream fsOutStream = new FileStream(_outPath+ "\\comp.arc",   FileMode.Create))
            {
                int bytesRead = -1;
                var totalReads = 0;
                var totalBytes = fsInStream.Length;
                byte[] bytes = new byte[bufferSize];
                int prevPercent = 0;
                using (DeflateStream dfStream = new DeflateStream(fsInStream, CompressionMode.Decompress))
                {
                    while ((bytesRead = dfStream.Read(bytes, 0, bufferSize)) > 0)
                    {
                        fsOutStream.Write(bytes, 0, bytesRead);
                        //csEncrypt.Write(bytes, 0, bytesRead);
                        totalReads += bytesRead;
                        int percent = System.Convert.ToInt32(((decimal)totalReads / (decimal)totalBytes) * 100);
                        if (percent != prevPercent)
                        {
                            AValue = "Decompressing Archive";
                            _compWorker.ReportProgress(percent);
                            prevPercent = percent;
                        }
                    }
            
                }
            }
            //File.Delete(_inPaths[0]);
            using (ZipArchive archive = ZipFile.OpenRead(_outPath + "\\comp.arc"))
            {
                ZipArchiveEntry[] entries = archive.Entries.ToArray();
                var fileCount = entries.Length;
                var processed = 0;
                int prevPercent = -1;
                foreach (var item in entries)
                {
                    item.ExtractToFile(Path.Combine(_outPath, item.FullName));
                  
                    processed++;
                    int percent = System.Convert.ToInt32(((decimal)processed / (decimal)fileCount) * 100);
                    if (percent != prevPercent)
                    {
                        AValue = "Extracting " + processed.ToString() + " of " + fileCount.ToString();
                        _compWorker.ReportProgress(percent);
                        prevPercent = percent;
                    }
                }
            }
            WipeFile wipe = new WipeFile();
            AValue = "Cleaning";
            wipe.SecureDelete(_outPath + "\\comp.arc", 3);    
        }

        public event ProgressChangedEventHandler ProgressChanged
        {
            add { _compWorker.ProgressChanged += value; }
            remove { _compWorker.ProgressChanged -= value; }
        }

        public event RunWorkerCompletedEventHandler Completed
        {
            add { _compWorker.RunWorkerCompleted += value; }
            remove { _compWorker.RunWorkerCompleted -= value; }
        }
        public void StartCompression()
        {
            _compWorker.RunWorkerAsync();
        }

    }
}
