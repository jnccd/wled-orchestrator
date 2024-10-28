using Server;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterServices();

var app = builder.Build();
app.RegisterEndpoints(app.Services);
app.Services.StartBackgroundServices();
app.Run();