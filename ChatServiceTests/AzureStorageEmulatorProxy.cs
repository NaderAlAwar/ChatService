using System;
using System.Diagnostics;
using System.IO;

namespace ChatServiceTests
{
    public class AzureStorageEmulatorProxy
    {
        private const string DefaultStorageEmulatorLocation = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";

        public AzureStorageEmulatorProxy()
        {
            Trace.TraceInformation("The Storage Emulator Location is " + DefaultStorageEmulatorLocation);
            if (!File.Exists(DefaultStorageEmulatorLocation))
            {
                throw new Exception("Could not find the storage emulator exe at " + DefaultStorageEmulatorLocation);
            }
        }

        public void StartEmulator()
        {
            ExecuteCommandOnEmulator("start");
        }

        public void StopEmulator()
        {
            ExecuteCommandOnEmulator("stop");
        }

        public void ClearAll()
        {
            ExecuteCommandOnEmulator("clear all");
        }

        private void ExecuteCommandOnEmulator(string arguments) 
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = DefaultStorageEmulatorLocation,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process proc = new Process
            {
                StartInfo = start
            };
            proc.Start();
            proc.WaitForExit();
        }
    }
}