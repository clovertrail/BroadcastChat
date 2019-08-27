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
        private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(100);
        private HubLifetimeManager<THub> _internalLifetimeManager;
        private System.Timers.Timer _timer;
        private bool _started;
        private string _recvMethod = "OOMCallback";
        private IHubCallerClients _callerClients;
        private Dictionary<string, string> _groupData = new Dictionary<string, string>();

        public PersistentLifetimeManager(HubLifetimeManager<THub> hubLifetimeManager)
        {
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
                /*
                foreach (var d in _data)
                {
                    if (_callerClients != null)
                    {
                        await _callerClients.All.SendAsync(_recvMethod, new[] { "OOMCheck", d });
                    }
                }
                */
                string key = null, value = null;
                foreach (var d in _groupData)
                {
                    if (_callerClients != null)
                    {
                        key = d.Key;
                        value = d.Value;
                        await _callerClients.Group(d.Key).SendAsync(_recvMethod, new[] { d.Key, d.Value});
                    }
                }
                if (key != null && value != null)
                {
                    await _callerClients.All.SendAsync(_recvMethod, new[] { key, value});
                }
            }
        }
    }
}

