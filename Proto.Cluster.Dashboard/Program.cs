using Proto;
using Proto.Cluster;
using Proto.Cluster.PartitionActivator;
using Proto.Cluster.Testing;
using Proto.Remote.GrpcNet;

var agent = new InMemAgent();
var lookup = new PartitionActivatorLookup();
var system = await GetSystem(agent, lookup);
var system2 = await GetSystem(agent, lookup);
var system3 = await GetSystem(agent, lookup);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton(system);
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
app.Run();

async Task<ActorSystem> GetSystem(InMemAgent agent, PartitionActivatorLookup partitionActivatorLookup)
{
    var provider = new TestProvider(new TestProviderOptions(), agent);
    var actorSystem = new ActorSystem().WithRemote(GrpcNetRemoteConfig.BindToLocalhost()).WithCluster(ClusterConfig.Setup("cluster", provider, partitionActivatorLookup).WithClusterKind("SomeKind", Props.Empty).WithClusterKind("SomeOtherKind", Props.Empty));
    await actorSystem.Cluster().StartMemberAsync();
    return actorSystem;
}