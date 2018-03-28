using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBoX
{
    class LoadKey
    {
        public void Load(string stream, out byte[] key, out byte[] iV)
        {
            if (stream == null || stream.Length <= 0)
                throw new ArgumentNullException("fileLocation");
            string[] seperators = new string[] { "fuckyou" };
            string[] bothKeys = stream.Split(seperators,StringSplitOptions.None);
            Console.WriteLine(bothKeys.Length);
            Console.ReadLine();
            iV = Encoding.UTF8.GetBytes(bothKeys[0]);
            key = Encoding.UTF8.GetBytes(bothKeys[1]);
        }
    }
}
