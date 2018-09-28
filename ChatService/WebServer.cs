using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ChatService
{
    public class WebServer
    {
        public static IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>();
        }
    }
    
}
