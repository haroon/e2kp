using System;
using System.Collections.Generic;
using System.IO;

namespace e2kp
{
	class Program
	{
		static void Main(string[] args)
		{
            string strEWalletFilePath = CommandLineParser.GetInFilePath(args);
            string strKeePassFilePath = CommandLineParser.GetOutFilePath(args);

            FileOps.ProcessInputFile(strEWalletFilePath, strKeePassFilePath);
        }
    }
}
