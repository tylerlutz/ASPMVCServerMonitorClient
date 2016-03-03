using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

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

        }
    }
}
