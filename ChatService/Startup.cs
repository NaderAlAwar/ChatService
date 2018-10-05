using System;
using System.IO;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            if (azureStorageSettings.ConnectionString == null)
            {
                Console.WriteLine("asdasd");
            }

            AzureCloudTable profileCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.ProfilesTableName);
            AzureTableProfileStore profileStore = new AzureTableProfileStore(profileCloudTable);
            services.AddSingleton<IProfileStore>(profileStore);

            AzureCloudTable usersCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.UsersTableName);
            AzureTableUserStore usersStore = new AzureTableUserStore(usersCloudTable);
            services.AddSingleton<IConversationsStore>(usersStore);

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
