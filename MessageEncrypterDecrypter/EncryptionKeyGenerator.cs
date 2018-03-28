using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Crypto;

namespace CryptoBoX
{
    class EncryptionKeyGenerator : IDisposable
    {
        private string salt = string.Empty;
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public string Salt { get => salt; set => salt = value; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        public void GenerateKey(string password, out byte[] key, out byte[]iV)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                SaltByte salt = new SaltByte();
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt.Salt);
                Rfc2898DeriveBytes p = new Rfc2898DeriveBytes(password, saltBytes);
                myRijndael.Key = p.GetBytes(myRijndael.KeySize/8);
                myRijndael.IV = p.GetBytes(myRijndael.BlockSize/8);
                key = myRijndael.Key;
                iV = myRijndael.IV;
            }
        }
    }
}
