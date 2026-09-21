using System.Threading.RateLimiting;
using WebPageSublimation.Components;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Auth;
using WebPageSublimation.Features.Promotores;
using WebPageSublimation.Features.Catalogo;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Proformas;
using WebPageSublimation.Features.SolicitudesPublicas;
using WebPageSublimation.Features.Pedidos;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddPolicy("public-form", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
});

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Deployment:BehindProxy")) app.UseForwardedHeaders();

await app.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseRateLimiter();

app.MapStaticAssets();
app.MapAccountEndpoints();
app.MapPromotorEndpoints();
app.MapCatalogoEndpoints();
app.MapClienteEndpoints();
app.MapProformaEndpoints();
app.MapSolicitudPublicaEndpoints();
app.MapPedidoEndpoints();
app.MapGet("/health", async (IDbContextFactory<AppDbContext> factory, CancellationToken ct) =>
{
    await using var db = await factory.CreateDbContextAsync(ct);
    return await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503);
}).ExcludeFromDescription();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (args.Contains("--import-catalog-assets", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var result = await CatalogoAssetImporter.ImportAsync(db, app.Environment.ContentRootPath);
    Console.WriteLine($"Importación terminada: {result.ProductsCreated} productos nuevos, {result.ImagesCreated} imágenes nuevas, {result.DraftsProcessed} borradores procesados.");
    return;
}

app.Run();
