using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Dynamic;

namespace fileDuplicates.Models
{
    class FileInfo
    {
        public String name { get; }
        public String parent { get; }
        public String checksum { get; }

        public FileInfo(System.IO.FileInfo file) {

            name = file.Name;
            parent = file.DirectoryName;
            checksum = CalculateMD5(file.FullName);
        }

        static String CalculateMD5(String filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
