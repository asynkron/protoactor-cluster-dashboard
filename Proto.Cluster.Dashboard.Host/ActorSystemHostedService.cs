﻿using Proto.Cluster.Seed;
using Proto.Utils;

namespace Proto.Cluster.Dashboard
{

    public class ActorSystemHostedService : IHostedService
    {
        private readonly ActorSystem _actorSystem;

        public ActorSystemHostedService(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _actorSystem.Cluster().StartMemberAsync();
            await Retry.Try(() => _actorSystem.Cluster().JoinSeed("localhost", 8099), Retry.Forever);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _actorSystem.Cluster().ShutdownAsync();
        }
    }
}