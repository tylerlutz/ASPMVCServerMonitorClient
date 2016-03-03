using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Management;

namespace ASPMVCServerMonitorClient
{
    class Program
    {
        static bool _running = true;
        static PerformanceCounter _cpuCounter, _memUsageCounter;

        static void Main(string[] args)
        {
            Thread pollingThread = null;

            Console.WriteLine("Server Monitoring Client: Reporting Server Usage");

            try
            {
                _cpuCounter = new PerformanceCounter();
                _cpuCounter.CategoryName = "Processor";
                _cpuCounter.CounterName = "% Processor Time";
                _cpuCounter.InstanceName = "_Total";

                _memUsageCounter = new PerformanceCounter("Memory", "Available KBytes");

                pollingThread = new Thread(new ParameterizedThreadStart(RunPollingThread));
                pollingThread.Start();

                Console.WriteLine("Press a key to stop and exit");
                Console.ReadKey();

                Console.WriteLine("Stopping Thread...");
                _running = false;

                pollingThread.Join(5000);
            }
            catch (Exception)
            {
                pollingThread.Abort();
                throw;
            }
        }

        static void RunPollingThread(object data)
        {
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");

            while (_running)
            {
                if((DateTime.Now - lastPollTime).TotalMilliseconds >= 1000)
                {
                    double cpuTime;
                    ulong memUsage, totalMemory;

                    GetMetrics(out cpuTime, out memUsage, out totalMemory);

                    var postData = new
                    {
                        MachineName = System.Environment.MachineName,
                        Processor = cpuTime,
                        MemUsage = memUsage,
                        TotalMemory = totalMemory
                    };

                    var json = JsonConvert.SerializeObject(postData);

                    var serverUrl = new Uri(ConfigurationManager.AppSettings["ServerUrl"]);

                    var client = new WebClient();
                    client.Headers.Add("Content-Type", "application/json");
                    client.UploadString(serverUrl, json);

                    lastPollTime = DateTime.Now;
                } else
                {
                    Thread.Sleep(10);
                }
            }
        }

        static void GetMetrics(out double processorTime, out ulong memUsage, out ulong totalMemory)
        {
            processorTime = (double)_cpuCounter.NextValue();
            memUsage = (ulong)_memUsageCounter.NextValue();
            totalMemory = 0;

            // Get total memory from WMI
            ObjectQuery memQuery = new ObjectQuery("SELECT * FROM CIM_OperatingSystem");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(memQuery);

            foreach (ManagementObject item in searcher.Get())
            {
                totalMemory = (ulong)item["TotalVisibleMemorySize"];
            }
        }
    }
}
