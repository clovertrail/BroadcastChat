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
        private string[] _dataFile = { "RecieveExchangeListSmartArbitrage.json", "RecieveProfitIndicatorArbitrage.json" };
        private List<string> _data = new List<string>();
        private string _recvMethod = "OOMCallback";
        private IHubCallerClients _callerClients;

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
            foreach (var df in _dataFile)
            {
                if (File.Exists(df))
                {
                    _data.Add(File.ReadAllText(df));
                    Console.WriteLine($"load data from {df}");
                }
            }
        }

        private async Task StartBroadcastInternal()
        {
            if (_started)
            {
                foreach (var d in _data)
                {
                    if (_callerClients != null)
                    {
                        await _callerClients.All.SendAsync(_recvMethod, new[] { "OOMCheck", d });
                    }
                    //await _internalLifetimeManager.SendAllAsync(_recvMethod, new[] { "OOMCheck", d});
                    //Console.WriteLine($"send data {d.Length}");
                }
            }
        }
    }
}

