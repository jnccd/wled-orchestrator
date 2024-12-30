using Server;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureWebhost();
builder.RegisterServices();

var app = builder.Build();
app.EnableRequestLogging();
app.EnableSwagger();
app.RegisterEndpoints();
app.Services.StartBackgroundServices();
app.Run();