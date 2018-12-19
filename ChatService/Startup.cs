using System;
using Microsoft.AspNetCore.Mvc.Versioning;
using ChatService.FaultTolerance;
using ChatService.Notifications;
using ChatService.Services;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatService.Storage.FaultTolerance;
using ChatService.Storage.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            services.AddApiVersioning(
                o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                } );

            var azureStorageSettings = GetSettings<AzureStorageSettings>();
            var notificationServiceSettings = GetSettings<NotificationServiceSettings>();
            var faultToleranceSettings = GetSettings<FaultToleranceSettings>();

            services.AddSingleton<IMetricsClient>(context =>
            {
                var metricsClientFactory = new MetricsClientFactory(context.GetRequiredService<ILoggerFactory>(),
                    TimeSpan.FromSeconds(15));
                return metricsClientFactory.CreateMetricsClient<LoggerMetricsClient>();
            });

            TimeoutPolicy timeoutPolicy = Policy.Timeout(faultToleranceSettings.TimeoutLength, TimeoutStrategy.Pessimistic);
            CircuitBreakerPolicy circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(
                    exceptionsAllowedBeforeBreaking: faultToleranceSettings.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromMinutes(faultToleranceSettings.DurationOfBreakInMinutes)
                );
            PolicyWrap policyWrap = Policy.Wrap(circuitBreakerPolicy, timeoutPolicy);
            services.AddSingleton<ISyncPolicy>(policyWrap);

            QueueClient queueClient = new QueueClient(notificationServiceSettings.ServiceBusConnectionString,
                notificationServiceSettings.QueueName);
            ServiceBusNotificationServiceClient notificationService = new ServiceBusNotificationServiceClient(queueClient);
            services.AddSingleton<INotificationService>(context =>
                new NotificationServiceMetricsDecorator(
                    new NotificationServiceFaultToleranceDecorator(
                        notificationService,
                        context.GetRequiredService<ISyncPolicy>()),
                    context.GetRequiredService<IMetricsClient>()));

            AzureCloudTable profileCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.ProfilesTableName);
            AzureTableProfileStore profileStore = new AzureTableProfileStore(profileCloudTable);
            services.AddSingleton<IProfileStore>(context => 
                new ProfileStoreMetricsDecorator(
                    new ProfileStoreFaultToleranceDecorator(
                        profileStore,
                        context.GetRequiredService<ISyncPolicy>()),
                    context.GetRequiredService<IMetricsClient>()));

            AzureCloudTable messagesCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.MessagesTableName);
            AzureTableMessagesStore messagesStore = new AzureTableMessagesStore(messagesCloudTable);
            services.AddSingleton<IMessagesStore>(context => 
                new MessagesStoreMetricsDecorator(
                    new MessagesStoreFaultToleranceDecorator(
                        messagesStore,
                        context.GetRequiredService<ISyncPolicy>()),
                    context.GetRequiredService<IMetricsClient>()));

            AzureCloudTable conversationsCloudTable = new AzureCloudTable(azureStorageSettings.ConnectionString, azureStorageSettings.UsersTableName);
            AzureTableConversationsStore conversationsStore = new AzureTableConversationsStore(conversationsCloudTable, messagesStore);
            services.AddSingleton<IConversationsStore>(context => 
                new ConversationStoreMetricsDecorator(
                    new ConversationsStoreFaultToleranceDecorator(
                        conversationsStore,
                        context.GetRequiredService<ISyncPolicy>()),
                    context.GetRequiredService<IMetricsClient>()));

            services.AddSingleton<IConversationService>(context =>
                new ConversationServiceMetricsDecorator(
                    new ConversationService(
                        context.GetRequiredService<IConversationsStore>(),
                        context.GetRequiredService<ILogger<ConversationService>>(),
                        context.GetRequiredService<INotificationService>()),
                    context.GetRequiredService<IMetricsClient>()));

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
