using Proto;
using Proto.Cluster;
using Proto.Cluster.Identity;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.Remote.GrpcNet;
using MudBlazor.Services;
using Proto.Remote;

var agent = new InMemAgent();
var lookup = new PartitionIdentityLookup();
var system = await GetSystem(agent, lookup);

for (int i = 0; i < 3; i++)
{
    GetSystem(agent, lookup);    
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton(system);
builder.Services.AddMudServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
_ = Task.Run(async () =>
{
    var rnd = new Random();
    while (true)
    {
        await Task.Delay(10);
        await system.Cluster()
            .RequestAsync<DummyResponse>("id" + rnd.Next(1, 1000), "SomeKind", new DummyRequest(),
                CancellationTokens.FromSeconds(5));

        await system.Cluster()
            .RequestAsync<DummyResponse>("id" + rnd.Next(1, 200), "SomeOtherKind", new DummyRequest(),
                CancellationTokens.FromSeconds(5));
    }
});
app.Run();

async Task<ActorSystem> GetSystem(InMemAgent agent, IIdentityLookup identityLookup)
{
    var props = Props.FromProducer(() => new DummyActor());
    var provider = new TestProvider(new TestProviderOptions(), agent);
    var actorSystem = new ActorSystem()
        .WithRemote(GrpcNetRemoteConfig.BindToLocalhost().WithRemoteDiagnostics(true))
        .WithCluster(ClusterConfig.Setup("cluster", provider, identityLookup)
            .WithClusterKind("SomeKind", props)
            .WithClusterKind("SomeOtherKind", props));
    await actorSystem.Cluster().StartMemberAsync();
    return actorSystem;
}

public record DummyRequest;

public record DummyResponse;

public class DummyActor : IActor
{
    public async Task ReceiveAsync(IContext context)
    {
        await Task.Delay(20);
        if (context.Message is DummyRequest)
        {
            var id = context.Get<ClusterIdentity>();
            context.Respond(new DummyResponse());
        }
    }
}