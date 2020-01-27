using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Performance
{
    public class App
    {
        private int pid;
        private string path;
        private string fileName;
        private string processName;
        private string actualDate;
        public App()
        {
            InitVariables();
            GetData();
            //Init Variables
            int counterTime = 1000;
            const string SEPARATOR = "\t";
            this.processName = GetProcessInstanceName(this.pid);

            //Init Performance Counters
            var perfCounter = new PerformanceCounter("Process", "% Processor Time", processName);
            PerformanceCounter mem = new PerformanceCounter("Process", "Working Set - Private", processName);

            // Initialize to start capturing
            perfCounter.NextValue();
            mem.NextValue();

            //Write File Header
            string header = "Process Name" + SEPARATOR + "Seconds(ms)" + SEPARATOR + "CPU(%)" + SEPARATOR + "Memory(MB)" + Environment.NewLine;
            this.SaveOnFile(header);
            while (true)
            {
                // give some time to accumulate data
                Thread.Sleep(1000);

                float cpu = perfCounter.NextValue() / Environment.ProcessorCount;
                float memory = mem.NextValue();
                float memoryKB = memory / (float)1024f;
                float memoryMB = memoryKB / (float)1024f;
                double finalMemory = Truncate(memoryMB, 2);
                string data = processName + SEPARATOR + counterTime + SEPARATOR + cpu + SEPARATOR + finalMemory + Environment.NewLine;
                this.SaveOnFile(data);
                //Console.WriteLine(data);
                counterTime += 1000;
            }
        }

        private void InitVariables()
        {
            this.pid = 0;
            this.path = "logs";
            this.actualDate = DateTime.Now.ToString().Split(' ')[0].Replace('/', '-');
            this.fileName = "Report " + actualDate + ".txt";
            this.processName = "";
            
    }
        public static void Main()
        {
            App a = new App();
        }

        public static string GetProcessInstanceName(int process_id)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
            string[] instances = cat.GetInstanceNames();
            foreach (string instance in instances)
            {
                using (PerformanceCounter cnt = new PerformanceCounter("Process", "ID Process", instance, true))
                {
                    int val = (int)cnt.RawValue;
                    if (val == process_id)
                        return instance;
                }
            }
            throw new Exception("Could not find performance counter ");
        }

        public static double Truncate(double value, int decimales)
        {
            double aux_value = Math.Pow(10, decimales);
            return (Math.Truncate(value * aux_value) / aux_value);
        }

        private void SaveOnFile(string data)
        {

            if (!Directory.Exists(this.path)){
                Directory.CreateDirectory(this.path);
            }

            File.AppendAllText(this.path + "\\" + this.fileName, data);
            Console.WriteLine("[" + DateTime.Now.ToString() + "]" + "Save Data of process " + this.pid + " successful on file " + this.path + "\\" + this.fileName);
        }

        private void GetData()
        {
            Console.WriteLine("Performance monitoring program for a running process. \nPerformed by Carlos Andres Garnica.\n");
            Console.WriteLine("Enter the PID you want to monitor performance.This PID is obtained directly from the Windows task manager: ");
            Console.Write("PID>> ");
            this.pid = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("");
            
        }
    }
}
