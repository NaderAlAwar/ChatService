<<<<<<< 32085f2f7d2ebf48b3f2b09b4aea4cba5e185473
﻿using System;
using System.IO;
=======
﻿using ChatService.Providers;
>>>>>>> Add the needed functionality to allow the saving of messages in Azure
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
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath("C:\\Users\\Asus\\Desktop\\Lab4\\ChatService.Configuration")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
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

            AzureTableProfileStore profileStore = new AzureTableProfileStore(profileCloudTable);
            AzureTableMessageStore messageStore = new AzureTableMessageStore(messageCloudTable);
            services.AddSingleton<IProfileStore>(profileStore);
            services.AddSingleton<IMessageStore>(messageStore);

            AzureCloudTable usersCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.UsersTableName);
            AzureTableUserStore usersStore = new AzureTableUserStore(usersCloudTable);
            services.AddSingleton<IConversationsStore>(usersStore);

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
