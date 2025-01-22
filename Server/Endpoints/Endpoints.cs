namespace Server.Endpoints;

public static class Endpoints
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.RegisterFrontendStaticFiles();
        app.RegisterWledEndpoints();
        app.MapControllers();
        app.RegisterLedThemeTypeInfoEndpoints();
    }
}
