// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class Startup
    {
        private const string UseLocalSignalR = "useLocalSignalR";
        private bool _useLocalSignalR;

        public Startup(IConfiguration configuration)
        {
            _useLocalSignalR =
                Environment.GetEnvironmentVariable(UseLocalSignalR) == null ||
                Environment.GetEnvironmentVariable(UseLocalSignalR) == "" ||
                Environment.GetEnvironmentVariable(UseLocalSignalR) == "false" ? false : true;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            
            if (_useLocalSignalR)
            {
                services.AddSignalR();
            }
            else
            {
                services.AddSignalR().AddAzureSignalR();
            }
            services.AddSingleton<PersistentLifetimeManager<Hub>>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseFileServer();
            if (_useLocalSignalR)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<Chat>("/chat");
                });
            }
            else
            {
                app.UseAzureSignalR(routes =>
                {
                    routes.MapHub<Chat>("/chat");
                });
            }
        }
    }
}

