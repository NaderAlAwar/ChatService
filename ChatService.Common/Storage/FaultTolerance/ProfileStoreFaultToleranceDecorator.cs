using System.Threading.Tasks;
using Polly;

namespace ChatService.Storage.FaultTolerance
{
    public class ProfileStoreFaultToleranceDecorator : IProfileStore
    {
        private readonly IProfileStore store;
        private readonly ISyncPolicy faultTolerancePolicy;

        public ProfileStoreFaultToleranceDecorator(IProfileStore store, ISyncPolicy faultTolerancePolicy)
        {
            this.store = store;
            this.faultTolerancePolicy = faultTolerancePolicy;
        }

        public Task<UserProfile> GetProfile(string username)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.GetProfile(username)
            );
        }

        public Task AddProfile(UserProfile profile)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.AddProfile(profile)
            );
        }

        public Task UpdateProfile(UserProfile profile)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.UpdateProfile(profile)
            );
        }

        public Task<bool> TryDelete(string username)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.TryDelete(username)
            );
        }
    }
}