using GraphQLUserApi.Data;
using GraphQLUserApi.GraphQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UserDb"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//builder.Services.AddMicrosoftIdentityWebApiAuthentication(azureAdConfig, "AzureAd", "Bearer");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "DualScheme";
    //options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie("Cookies", options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie expiration time
})
.AddOpenIdConnect("OpenIdConnect", options =>
{
    options.Authority = azureAdConfig["Authority"];
    options.ClientId = azureAdConfig["ClientId"];
    options.ResponseType = OpenIdConnectResponseType.Code; ; // Authorization Code Flow
    options.SaveTokens = true;

    options.ClientSecret = azureAdConfig["ClientSecret"]; // 🔑 Required for "Web" app

    // Scopes
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    //options.Scope.Add("User.Read");

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.ValidateIssuer = true;

    options.Events.OnTokenValidated = context =>
    {
        Console.WriteLine($"Token Received and validated {context.TokenEndpointResponse.AccessToken}");
        return Task.CompletedTask;
    };
})
.AddJwtBearer("Bearer", options =>
{
    options.Authority = $"{azureAdConfig["Authority"]}";
    options.Audience = azureAdConfig["ClientId"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = $"{azureAdConfig["Authority"]}",
        ValidateAudience = true,
        ValidateLifetime = true
    };

    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Log authentication failures
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
})
.AddPolicyScheme("DualScheme", "DualScheme", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        return authHeader?.StartsWith("Bearer ") == true ? "Bearer" : "Cookies";
    };

    options.ForwardChallenge = "DualChallengeHandler";
})
.AddPolicyScheme("DualChallengeHandler", "DualChallengeHandler", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        return authHeader?.StartsWith("Bearer ") == true ? "Bearer" : "OpenIdConnect";
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});


builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapGraphQL().RequireAuthorization("Authenticated");

app.Use(async (context, next) =>
{
    Console.WriteLine($"{context.Request.Path}");
    await next();
});

app.Run();
