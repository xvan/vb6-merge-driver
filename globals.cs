using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace vb6_merge_driver
{
    static class globals
    {
        public static string RootDir { get; private set; }
        public static string FileName { get; private set;  }
        public static string FileExtension { get; private set; }
        public static bool VbpMode { 
            get { return FileExtension.Equals(".vbp", StringComparison.InvariantCultureIgnoreCase); }
        }

        public static void initialize(string filepath) {
            RootDir = Path.GetDirectoryName(filepath);
            FileName = Path.GetFileName(filepath);
            FileExtension = Path.GetExtension(filepath);
        }
    }
}
