using Server.Helper;

namespace Server
{
    public static class WledOrchestratorEndpoints
    {
        public static void RegisterEndpoints(this IEndpointRouteBuilder routes, IServiceProvider services)
        {
            routes.MapGet("/hewwo", () =>
            {
                return Results.Extensions.Html(@$"<!doctype html>
                    <html>
                        <head>
                            <title>Hewwo</title>
                            <style>
                                body {{font-family: sans-serif;}}
                            </style>
                        </head>
                        <body>
                            <h1>Hewwo Wowld :3</h1>
                        </body>
                    </html>");
            });
        }
    }
}