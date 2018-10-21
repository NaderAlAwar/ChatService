using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.EventFlow;
using Microsoft.Diagnostics.EventFlow.Inputs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService
{
    public class WebServer
    {
        public static IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>();
        }

        public void Run()
        {
            IWebHost webHost = CreateWebHostBuilder().Build();
            IConfiguration configuration = webHost.Services.GetRequiredService<IConfiguration>();
            IConfiguration eventFlowConfig = configuration.GetSection("EventFlowConfig");
            using (var pipeline = DiagnosticPipelineFactory.CreatePipeline(eventFlowConfig))
            {
                ILoggerFactory loggerFactory = webHost.Services.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddEventFlow(pipeline);
                webHost.Run();
            }
        }
    }
    
}
