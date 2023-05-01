﻿/* Copyright 2009 HPDI, LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace Hpdi.Vss2Git
{
    enum ErrorCode : ushort
    {
        None = 0,
        Unknown = 1,
        FileNotFound = 100,
        InvalidSettings = 200
    }

    /// <summary>
    /// Entrypoint to the application.
    /// </summary>
    /// <author>Trevor Robinson</author>
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // Only accepts a single CLI argument in the form of a settings text file
            if (args.Length == 1)
            {
                string filePath = args[0];
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File provided does not exist: {filePath}");
                    return (int) ErrorCode.FileNotFound;
                }

                var parsedResults = MainExecution.Instance.ImportSettings(filePath);
                if (parsedResults.Count >= 1)
                {
                    Console.WriteLine($"Found the following errors with the provided file: {filePath}");

                    foreach (var result in parsedResults)
                    {
                        string underscoreCased = Regex.Replace(result.Item2, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", "_$1").ToUpper();
                        Console.WriteLine($"{result.Item1}: {underscoreCased}");
                    }
                    return (int) ErrorCode.InvalidSettings;
                }

                try
                {
                    MainExecution.Instance.StartConversion();

                    while (!MainExecution.Instance.isWorkQueueIdle())
                    {
                        // Wait here until work queue is idle
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return (int) ErrorCode.Unknown;
                }
            }
            else
            {
                FreeConsole();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }

            return 0;
        }


        ///
        /// Lets me hide the console part of the app when running in WinForms mode
        ///
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int FreeConsole();
    }

}
