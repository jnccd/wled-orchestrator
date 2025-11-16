using Server;
using Server.Endpoints;

Console.WriteLine("Hewwo!");
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureWebhost();
builder.RegisterServices();

var app = builder.Build();
app.AddRequestLoggingMiddleware();
app.ConfigureWebApp();
app.RegisterEndpoints();
app.Services.StartBackgroundServices();
app.Run();