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
        public String fullPath { get; }
        public String name { get; }
        public String parent { get; }
        public String checksum
        {
            get
            {
                return CalculateMD5();
            }
        }

        public FileInfo(System.IO.FileInfo file) {
            fullPath = file.FullName;
            name = file.Name;
            parent = file.DirectoryName;
            //checksum = System.Threading.Tasks.Task.Run(() => CalculateMD5(file.FullName)).Result;
        }

        String CalculateMD5()
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(fullPath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
