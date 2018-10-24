using Microsoft.Extensions.Logging.Metrics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Storage.Metrics
{
    public class ProfileStoreMetricsDecorator : IProfileStore
    {
        private readonly IProfileStore store;
        private readonly AggregateMetric addProfileMetric;
        private readonly AggregateMetric getProfileMetric;
        private readonly AggregateMetric tryDeleteMetric;
        private readonly AggregateMetric updateProfileMetric;

        public ProfileStoreMetricsDecorator(IProfileStore store, IMetricsClient metricsClient)
        {
            this.store = store;

            addProfileMetric = metricsClient.CreateAggregateMetric("AddProfileTime");
            getProfileMetric = metricsClient.CreateAggregateMetric("GetProfileTime");
            tryDeleteMetric = metricsClient.CreateAggregateMetric("TryDeleteTime");
            updateProfileMetric = metricsClient.CreateAggregateMetric("UpdateProfileTime");
        }

        public Task AddProfile(UserProfile profile)
        {
            return addProfileMetric.TrackTime(() => store.AddProfile(profile));
        }

        public Task<UserProfile> GetProfile(string username)
        {
            return getProfileMetric.TrackTime(() => store.GetProfile(username));
        }

        public Task<bool> TryDelete(string username)
        {
            return tryDeleteMetric.TrackTime(() => store.TryDelete(username));
        }

        public Task UpdateProfile(UserProfile profile)
        {
            return updateProfileMetric.TrackTime(() => store.UpdateProfile(profile));
        }
    }
}
