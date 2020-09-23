﻿using Materal.NetworkHelper;
using Materal.WindowsHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Materal.ConsoleUI
{
    internal class Program
    {
        public static async Task Main()
        {
            const string path = @"E:\Project\Materal\Application\Materal.APP\Deploy.Server\Application\template\Start.cmd";
            var cmdManager = new CmdManager();
            cmdManager.OutputDataReceived += CmdManager_OutputDataReceived;
            cmdManager.ErrorDataReceived += CmdManager_ErrorDataReceived;
            await cmdManager.RunCmdCommandsAsync(
                @"cd Application\template",
                @"npm run deploy");
            //await cmdManager.RunCmdCommandsAsync("E:", @"cd Project\Materal\Application\Materal.APP\Deploy.Server\Application\template", "echo Hello World");
            //await cmdManager.RunCmdCommandsAsync("echo Hello World");
            Console.ReadKey();
        }

        private static void CmdManager_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void CmdManager_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
