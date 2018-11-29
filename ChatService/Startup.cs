using System;
using ChatService.Notifications;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatService.Storage.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            AzureStorageSettings azureStorageSettings = GetStorageSettings();
            NotificationsServiceSettings notificationsServiceSettings = GetNotificationsSettings();

            services.AddSingleton<IMetricsClient>(context =>
            {
                var metricsClientFactory = new MetricsClientFactory(context.GetRequiredService<ILoggerFactory>(),
                    TimeSpan.FromSeconds(15));
                return metricsClientFactory.CreateMetricsClient<LoggerMetricsClient>();
            });

            services.AddSingleton<INotificationsService>(new NotificationService(notificationsServiceSettings.BaseUri));

            AzureCloudTable profileCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.ProfilesTableName);
            AzureTableProfileStore profileStore = new AzureTableProfileStore(profileCloudTable);
            services.AddSingleton<IProfileStore>(context => 
                new ProfileStoreMetricsDecorator(profileStore, context.GetRequiredService<IMetricsClient>()));

            AzureCloudTable messagesCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.MessagesTableName);
            AzureTableMessagesStore messagesStore = new AzureTableMessagesStore(messagesCloudTable);
            services.AddSingleton<IMessagesStore>(context => 
                new MessagesStoreMetricsDecorator(messagesStore, context.GetRequiredService<IMetricsClient>()));

            AzureCloudTable conversationsCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.UsersTableName);
            AzureTableConversationsStore conversationsStore = new AzureTableConversationsStore(conversationsCloudTable, messagesStore);
            services.AddSingleton<IConversationsStore>(context => 
                new ConversationStoreMetricsDecorator(conversationsStore, context.GetRequiredService<IMetricsClient>()));

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

        private NotificationsServiceSettings GetNotificationsSettings()
        {
            IConfiguration notificationsConfiguration = Configuration.GetSection(nameof(NotificationsServiceSettings));
            NotificationsServiceSettings notificationsSettings = new NotificationsServiceSettings();
            notificationsConfiguration.Bind(notificationsSettings);
            return notificationsSettings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                loggerFactory.AddConsole(); // add console log provider
                loggerFactory.AddDebug(); // add debug log provider
            }

            app.UseMvc();
        }
    }
}
