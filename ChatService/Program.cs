using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ChatService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebServer().Run();
        }
    }
    
}
