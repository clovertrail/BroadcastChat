// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class Program
    {
        private const int DefaultPort = 5050;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static readonly Action<WebHostBuilderContext, KestrelServerOptions> KestrelConfig =
            (context, options) =>
            {
                var config = context.Configuration;
                if (!int.TryParse(config["Port"], out var port))
                {
                    port = DefaultPort;
                }
                options.ListenAnyIP(port);
            };

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(KestrelConfig)
                   .UseStartup<Startup>();
    }
}
