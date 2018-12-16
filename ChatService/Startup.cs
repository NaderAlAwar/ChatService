﻿using System;
using ChatService.FaultTolerance;
using ChatService.Notifications;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatService.Storage.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;

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

            var azureStorageSettings = GetSettings<AzureStorageSettings>();
            var notificationServiceSettings = GetSettings<NotificationServiceSettings>();
            var faultToleranceSettings = GetSettings<FaultToleranceSettings>();

            services.AddSingleton<IMetricsClient>(context =>
            {
                var metricsClientFactory = new MetricsClientFactory(context.GetRequiredService<ILoggerFactory>(),
                    TimeSpan.FromSeconds(15));
                return metricsClientFactory.CreateMetricsClient<LoggerMetricsClient>();
            });

            TimeoutPolicy timeoutPolicy = Policy.TimeoutAsync(faultToleranceSettings.TimeoutLength, TimeoutStrategy.Pessimistic);
            CircuitBreakerPolicy circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: faultToleranceSettings.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromMinutes(faultToleranceSettings.DurationOfBreakInMinutes)
                );
            PolicyWrap policyWrap = Policy.Wrap(circuitBreakerPolicy, timeoutPolicy);
            services.AddSingleton<IAsyncPolicy>(policyWrap);

            QueueClient queueClient = new QueueClient(notificationServiceSettings.ServiceBusConnectionString,
                notificationServiceSettings.QueueName);
            ServiceBusNotificationServiceClient notificationService = new ServiceBusNotificationServiceClient(queueClient);
            services.AddSingleton<INotificationService>(context =>
                new NotificationServiceMetricsDecorator(notificationService, context.GetRequiredService<IMetricsClient>()));

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

        private T GetSettings<T>() where T : new()
        {
            var config = Configuration.GetSection((typeof(T).Name));
            T settings = new T();
            config.Bind(settings);
            return settings;
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
