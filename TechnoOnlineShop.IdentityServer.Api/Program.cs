using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using TechnoOnlineShop.IdentityServer.Application;
using TechnoOnlineShop.IdentityServer.Application.Contracts;
using TechnoOnlineShop.IdentityServer.Application.Contracts.AppEvents;
using TechnoOnlineShop.IdentityServer.Application.Contracts.Options;
using TechnoOnlineShop.IdentityServer.Infrastructure.EF;
using static OpenIddict.Abstractions.OpenIddictConstants;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();




builder.Services.AddDbContext<IdentityProviderDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityDatabase")//,
        //sql => sql.MigrationsAssembly(
        //    "TechnoOnlineShop.IdentityServer.Infrastructure.EF")
            );

    options.UseOpenIddict();


});


//var issuer = builder.Configuration["OpenIddict:issuer"];

//builder.Services.AddOpenIddict()
//    .AddCore(options =>
//    {
//        options.UseEntityFrameworkCore()
//               .UseDbContext<IdentityProviderDbContext>();
//    })
//    .AddServer(options =>
//    {

//        var loggerFactory = builder.Services.BuildServiceProvider()
//    .GetRequiredService<ILoggerFactory>();

//        var logger = loggerFactory.CreateLogger("OpenIddict");
       
//        try
//        {
//            var certificatePath = Path.Combine(
//                AppContext.BaseDirectory,
//                "certificates");

//            // Load persistent signing certificate
//            var signingCertificate = X509Certificate2.CreateFromPemFile(
//                Path.Combine(certificatePath, "signing-cert-cert.pem"),
//                Path.Combine(certificatePath, "signing-cert-private.pem"));

//            // Configure issuer
//            var issuer = builder.Configuration["OpenIddict:Issuer"]
//                ?? throw new InvalidOperationException("OpenIddict issuer is not configured.");

//            options.SetIssuer(new Uri(issuer, UriKind.Absolute));

//            // Endpoints
//            options.SetAuthorizationEndpointUris("/connect/authorize");
//            options.SetTokenEndpointUris("/connect/token");
//            options.SetUserInfoEndpointUris("/connect/userinfo");
//            options.SetEndSessionEndpointUris("/connect/logout");

//            options.SetConfigurationEndpointUris(
//                "/.well-known/openid-configuration");

//            options.SetJsonWebKeySetEndpointUris(
//                "/.well-known/jwks.json");

//            // Authorization flows
//            options.AllowAuthorizationCodeFlow();
//            options.RequireProofKeyForCodeExchange();

//            // Scopes
//            options.RegisterScopes(
//                Scopes.OpenId,
//                Scopes.Profile,
//                "orders.read",
//                "orders.settle");

//            // Signing certificate
//            options.AddSigningCertificate(signingCertificate);

//            // Environment-specific encryption certificate
//            if (builder.Environment.IsDevelopment())
//            {
//                logger.LogInformation("development: true");
//                options.AddDevelopmentEncryptionCertificate();
//            }
//            else
//            {
//                var encryptionCertificate = X509Certificate2.CreateFromPemFile(
//                    Path.Combine(certificatePath, "encryption-cert-cert.pem"),
//                    Path.Combine(certificatePath, "encryption-cert-private.pem"));

//                options.AddEncryptionCertificate(encryptionCertificate);
//            }

//            // Resource APIs validate signed JWTs using JWKS
//            options.DisableAccessTokenEncryption();

//            // ASP.NET Core integration
//            options.UseAspNetCore()
//                   .EnableAuthorizationEndpointPassthrough()
//                   .EnableTokenEndpointPassthrough();
//        }
//        catch (Exception e)
//        {
//            logger.LogError(e,"Exception occured:{excep}", e.Message);
//        }

//    });


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
    "[SERILOG {Timestamp:HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}") //new CompactJsonFormatter()
    .CreateLogger();

builder.Services.AddLogging(logging =>
{
    //logging.ClearProviders();
    logging.AddSerilog();
});

builder.Host.UseSerilog();


//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        // Your IdentityProvider's issuer/discovery URL.
//        options.Authority =
//            "https://localhost:7001/";

//        // The audience for service-to-service API access tokens.
//        options.Audience =
//            "technoonlineshop-identityprovider";

//        options.RequireHttpsMetadata = true;

//        // Optional if your IdP uses non-standard claim mapping:
//        options.MapInboundClaims = false;
//    });

//builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<SmsChallengeOptions>(builder.Configuration.GetSection("SmsChallenge"));

// Register concrete implementations as you build them.
builder.Services.AddScoped<ISmsChallengeService, SmsChallengeService>();// : 
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);



//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("UserManagementService", policy =>
//    {
//        policy.RequireAuthenticatedUser();
//        policy.RequireClaim("client_id", "usermanagement-service");
//        policy.RequireClaim("scope", "identity.sms.request");
//    });

//    options.AddPolicy("BffService", policy =>
//    {
//        policy.RequireAuthenticatedUser();
//        policy.RequireClaim("client_id", "techno-bff");
//        policy.RequireClaim("scope", "identity.token.issue");
//    });
//});



 var app = builder.Build();


//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider
//        .GetRequiredService<IdentityProviderDbContext>();

//    var loggerFactory = builder.Services.BuildServiceProvider()
//    .GetRequiredService<ILoggerFactory>();

//    var logger = loggerFactory.CreateLogger("OpenIddict");
//    logger.LogInformation("Auto migration...");

//    await dbContext.Database.MigrateAsync();

//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();



app.Run();
