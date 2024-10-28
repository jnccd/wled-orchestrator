using Server;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureWebhost();
builder.RegisterServices();

var app = builder.Build();
app.RegisterEndpoints(app.Services);
app.Services.StartBackgroundServices();
app.Run();