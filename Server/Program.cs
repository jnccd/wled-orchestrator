using Server;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterServices();

var app = builder.Build();
app.RegisterWledOrchestratorEndpoints(app.Services);
app.Run();