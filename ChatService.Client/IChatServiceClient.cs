using ChatService.DataContracts;
using System.Threading.Tasks;

namespace ChatService.Client
{
    public interface IChatServiceClient
    {
        Task CreateProfile(CreateProfileDto profileDto);
    }
}
