using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace veeamTestTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //declare variables
            string sourcePath, replicaPath, logFilePath;
            int syncInterval;

            //checking if the arguments are valid
            if (!ValidateArguments(args, out sourcePath, out replicaPath, out syncInterval, out logFilePath))
            {
                return;
            }
            else
            {
                Console.WriteLine("All arguments were validated correctly!");
            }

            //cheking if the folders exists
            if (!FolderExists(sourcePath, replicaPath))
            {
                return;
            }

            //initialize the sync before the first timer
            SyncFolder(sourcePath, replicaPath, logFilePath);

            //setting sync to be periodically
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += (sender, e) => SyncFolder(sourcePath, replicaPath, logFilePath);
            aTimer.Interval = syncInterval * 1000; 
            aTimer.Enabled = true;
            

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                aTimer.Dispose();
                Console.WriteLine("Synchronization stopped. Exiting program...");
                Environment.Exit(0);
            };


            Console.WriteLine("Press \'q\' to quit the sample.");
            while (Console.ReadLine() != "q") { }

            Console.WriteLine("Synchronization stopped. Exiting program...");

        }

        //validate arguments
        static bool ValidateArguments(string[] args, out string sourcePath, out string replicaPath,out int syncInterval, out string logFilePath)
        {
            syncInterval = 0;
            sourcePath = "";
            replicaPath = "";
            logFilePath = "";

            //checking if we are passing four arguments
            if (args.Length != 4)
            {
                Console.WriteLine("You need to pass four arguments: source folder path, replica folder path, synchronization interval (s), and log file path");
                return false;
            }            

            //converting to absolute path
            try
            {
                sourcePath = Path.GetFullPath(args[0]);
                replicaPath = Path.GetFullPath(args[1]);
                logFilePath = Path.GetFullPath(args[3]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Invalid path provided. {ex.Message}");
                return false;
            }            

            //check if the syncInterval is valid
            if (!int.TryParse(args[2], out syncInterval) || syncInterval <= 0)
            {
                Console.WriteLine("Error: Synchronization interval must be a positive valid number.");
                return false;
            }            
            return true;
        }

        //check if the folders exists
        static bool FolderExists(string sourcePath, string replicaPath)
        {
            //checking if the source folder exist
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"Error: Source folder {sourcePath} doesn't exist. Please create one!");
                return false;
            }

            //checking if the replica folder exist and if not create one  
            try
            {
                if (!Directory.Exists(replicaPath))
                {
                    Console.WriteLine($"Alert: Replica folder {replicaPath} doesn't exist. Creating one...");
                    Directory.CreateDirectory(replicaPath);
                    Console.WriteLine("Replica folder created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating replica folder: {ex.Message}");
                return false;
            }
            return true;
        }

        //sincronize the replica folder with the source folder
        static void SyncFolder(string sourcePath, string replicaPath,string logFilePath)
        {
            try
            {
                //create directory that exist in source but not in replica
                foreach (string sourceDir in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {                   
                    string replicaDir = sourceDir.Replace(sourcePath, replicaPath);                    

                    if (!Directory.Exists(replicaDir))
                    {
                        Directory.CreateDirectory(replicaDir);
                        Log($"Directory {Path.GetFileName(replicaDir)} created in replica folder", logFilePath);
                    }
                }

                //copying new files or updating files
                foreach (string sourceFile in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {                        
                    string replicaFile = sourceFile.Replace(sourcePath, replicaPath);       
                    
                    if (!File.Exists(replicaFile))
                    {
                        File.Copy(sourceFile, replicaFile);
                        Log($"File {Path.GetFileName(replicaFile)} copied to replica folder", logFilePath);

                    }
                    else
                    {
                        string sourceMD5 = CalculateMD5(sourceFile);
                        string replicaMD5 = CalculateMD5(replicaFile);

                        if (!String.Equals(sourceMD5, replicaMD5))
                        {
                            File.Copy(sourceFile, replicaFile, true);
                            Log($"File {Path.GetFileName(replicaFile)} updated in replica folder", logFilePath);
                        }
                    }
                }

                //delete directories from replica that doesn't exist in source
                foreach (string replicaDir in Directory.GetDirectories(replicaPath, "*", SearchOption.AllDirectories))
                {
                    string sourceDir = replicaDir.Replace(replicaPath, sourcePath);
                    
                    if (!Directory.Exists(sourceDir))
                    {
                        Directory.Delete(replicaDir, true); 
                        Log($"Directory {Path.GetFileName(replicaDir)} deleted from replica folder", logFilePath);
                    }
                }

                //delete files from replica that doesn't exist in source
                foreach (string replicaFile in Directory.GetFiles(replicaPath, "*", SearchOption.AllDirectories))
                {
                    string sourceFile = replicaFile.Replace(replicaPath, sourcePath);                    

                    if (!File.Exists(sourceFile))
                    {
                        File.Delete(replicaFile);
                        Log($"File {Path.GetFileName(replicaFile)} deleted from replica folder", logFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        //get MD5 hash
        static string CalculateMD5(string filepath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filepath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        //write operations into log file
        static void Log(string message, string logFilePath)
        {
            string logMessage = $"{DateTime.Now}: {message}";
            Console.WriteLine(logMessage);
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
    }
}
