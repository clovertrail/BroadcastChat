// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class Chat : Hub
    {
        private PersistentLifetimeManager<Hub> _persistentLifetimeManger;

        public Chat(PersistentLifetimeManager<Hub> persistentLifetimeManger)
        {
            _persistentLifetimeManger = persistentLifetimeManger;
        }

        public override Task OnConnectedAsync()
        {
            _persistentLifetimeManger.StartBroadcast(Clients);
            return Task.CompletedTask;
        }

        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.All.SendAsync("joinedGroup", "joinedGroup", groupName);
            Console.WriteLine($"join group {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
