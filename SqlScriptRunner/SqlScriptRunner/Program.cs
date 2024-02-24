using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace SqlScriptRunner
{
    class Program
    {
        static string BasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        static string FileName = "SQL_Automation.ps1";
        static string powerShellEXE = "C:\\windows\\system32\\windowspowershell\\v1.0\\powershell.exe";
        static void Main(string[] args)
        {
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
            Console.ReadLine();
        }


        static void Init()
        {
            Console.Clear();
           // Console.WriteLine("Processing.....");

            if (File.Exists(powerShellEXE))
            {
                RunProcess(powerShellEXE);
            }
            else
            {
                Console.WriteLine("PoweShell EXE does not found in default path..");
                Console.WriteLine("Please enter PoweShell EXE path: ");
                powerShellEXE = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(powerShellEXE))
                {
                    if (File.Exists(powerShellEXE))
                    {
                        RunProcess(powerShellEXE);
                    }
                    else
                    {
                        Console.WriteLine("Given Path is incorrect");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Init();
                }
            }
        }

        static void RunProcess(string powerShellEXE)
        {
            Console.WriteLine("Connecting to PowerShell.....");
            Thread.Sleep(2000);
            string psSqlPath = PowerShell.GetFilePath();
            string poweShellCommand = @"Start-Process Powershell  -ArgumentList '-executionpolicy', 'bypass', '-NOEXIT','-File' ,'" + psSqlPath + "' -WindowStyle Maximized";


            Process.Start(powerShellEXE, poweShellCommand);
           
            Environment.Exit(0);
        }




    }
}
