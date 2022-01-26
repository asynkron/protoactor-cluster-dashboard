using MatBlazor;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Identity;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.Remote.GrpcNet;

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
builder.Services.AddMatBlazor();
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

async Task<ActorSystem> GetSystem(InMemAgent agent, IIdentityLookup identityLookup)
{
    var provider = new TestProvider(new TestProviderOptions(), agent);
    var actorSystem = new ActorSystem().WithRemote(GrpcNetRemoteConfig.BindToLocalhost()).WithCluster(ClusterConfig.Setup("cluster", provider, identityLookup).WithClusterKind("SomeKind", Props.Empty).WithClusterKind("SomeOtherKind", Props.Empty));
    await actorSystem.Cluster().StartMemberAsync();
    return actorSystem;
}