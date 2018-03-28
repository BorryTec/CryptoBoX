using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using ComponentAce.Compression.ZipForge;
using ComponentAce.Compression.Archiver;

namespace CryptoBoX
{
    public enum CompressionOption
    {
        Compress,
        Decompress,
    }

    class CompressFolder : IDisposable
    {
        private string[] _inPaths;
 
        private string _outPath;
        BackgroundWorker _compWorker;

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
            ZipForge archive = new ZipForge();
            archive.FileName = _outPath;
            archive.OpenArchive(FileMode.OpenOrCreate);
            archive.OnFileProgress += new BaseArchiver.OnFileProgressDelegate(archiver_OnFileProgress);
            var fileCount = _inPaths.Length;
            var processed = 0;
            int prevPercent = -1;
            foreach (var item in _inPaths)
            {
                archive.AddFiles(item);
                processed++;
                int percent = System.Convert.ToInt32(((decimal)processed / (decimal)fileCount) * 100);
                if (percent != prevPercent)
                {
                    _compWorker.ReportProgress(percent);
                    prevPercent = percent;
                }
            }
            archive.CloseArchive();
        }

        public void StartDecompression(object sender, DoWorkEventArgs e)
        {
            Directory.CreateDirectory(_outPath);
            ZipForge archive = new ZipForge();


            archive.FileName = _outPath + "\\file.dec";
            archive.OpenArchive(FileMode.OpenOrCreate);
            archive.OnFileProgress += new BaseArchiver.OnFileProgressDelegate(archiver_OnFileProgress);
            archive.BaseDir = _outPath;
            archive.ExtractFiles("*.*");
   
                
            archive.CloseArchive();
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
        void archiver_OnFileProgress(object sender, string fileName, double progress,
          TimeSpan timeElapsed, TimeSpan timeLeft, ProcessOperation operation,
          ProgressPhase progressPhase, ref bool cancel)
        {
           // _compWorker.ReportProgress(Convert.ToInt32((double)progress));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _compWorker.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CompressFolder() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
