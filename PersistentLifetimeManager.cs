using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class PersistentLifetimeManager<THub> where THub : Hub
    {
        private const string SendingInterval = "SendingInterval";
        private TimeSpan Interval;
        private HubLifetimeManager<THub> _internalLifetimeManager;
        private System.Timers.Timer _timer;
        private bool _started;
        private string _recvMethod = "OOMCallback";
        private IHubCallerClients _callerClients;
        private Dictionary<string, string> _groupData = new Dictionary<string, string>();

        public PersistentLifetimeManager(HubLifetimeManager<THub> hubLifetimeManager)
        {
            var intervalStr = Environment.GetEnvironmentVariable(SendingInterval);
            if (intervalStr != null)
            {
                var intervalInt = Convert.ToInt32(intervalStr);
                Interval = TimeSpan.FromMilliseconds(intervalInt);
            }
            else
            {
                Interval = TimeSpan.FromMilliseconds(100);
            }
            _internalLifetimeManager = hubLifetimeManager;
            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    await StartBroadcastInternal();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                }
            };
            _timer.Start();
            LoadData();
        }

        public void StartBroadcast(IHubCallerClients callerClients)
        {
            _started = true;
            _callerClients = callerClients;
            Console.WriteLine("Start broadcasting");
        }

        private void LoadData()
        {
            foreach (string p in Directory.EnumerateFiles("data", "*.json"))
            {
                var f = Path.GetFileName(p);
                var dot = f.IndexOf(".");
                var grp = f.Substring(0, dot);
                var value = File.ReadAllText(p);
                _groupData.Add(grp, value);
                Console.WriteLine($"load data from {f}");
            }
        }

        private async Task StartBroadcastInternal()
        {
            if (_started)
            {
                string key = null, value = null;
                foreach (var d in _groupData)
                {
                    if (_callerClients != null)
                    {
                        key = d.Key;
                        value = d.Value;
                        await _callerClients.Group(d.Key).SendCoreAsync(_recvMethod, new[] { d.Key, d.Value });
                    }
                }
                if (key != null && value != null)
                {
                    await _callerClients.All.SendCoreAsync(_recvMethod, new[] { key, value });
                }
            }
        }
    }
}

