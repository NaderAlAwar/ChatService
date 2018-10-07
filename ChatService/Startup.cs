﻿using System;
using System.IO;
﻿using ChatService.Providers;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ChatService
{
    public class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }
        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            AzureStorageSettings azureStorageSettings = GetStorageSettings();

            AzureCloudTable profileCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.ProfilesTableName);
            AzureCloudTable messageCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.MessagesTableName);
            AzureCloudTable usersCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.UsersTableName);

            AzureTableProfileStore profileStore = new AzureTableProfileStore(profileCloudTable);
            AzureTableUserStore usersStore = new AzureTableUserStore(usersCloudTable);
            AzureTableMessageStore messageStore = new AzureTableMessageStore(messageCloudTable, usersStore);

            services.AddSingleton<IConversationsStore>(usersStore);
            services.AddSingleton<IProfileStore>(profileStore);
            services.AddSingleton<IMessageStore>(messageStore);

            services.TryAddSingleton<ITimeProvider, UtcTimeProvider>();

            services.AddLogging();
            services.AddMvc();
        }

        private AzureStorageSettings GetStorageSettings()
        {
            IConfiguration storageConfiguration = Configuration.GetSection(nameof(AzureStorageSettings));
            AzureStorageSettings storageSettings = new AzureStorageSettings();
            storageConfiguration.Bind(storageSettings);
            return storageSettings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddConsole();
                loggerFactory.AddDebug();
            }

            app.UseMvc();
        }
    }
}
