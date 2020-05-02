using System;
using System.Collections.Generic;
using System.Text;

namespace e2kp
{
    class CommandLineParser
    {
        public static string GetInFilePath(string[] args)
        {
            return GetFilePath(args, 0, "eWallet.txt");
        }
        
        public static string GetOutFilePath(string[] args)
        {
            return GetFilePath(args, 1, "KeePass.xml");
        }
        
        static string GetFilePath(string[] args, int idx, string defaultPath)
        {
            if (args.Length > idx)
            {
                defaultPath = args[idx];
            }
            return defaultPath;
        }
    }
}
