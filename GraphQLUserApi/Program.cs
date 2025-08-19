using GraphQLUserApi.Data;
using GraphQLUserApi.GraphQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UserDb"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie("Cookies", options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Cookie expiration time
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
});
//builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HumanOnly", policy =>
        policy.AddAuthenticationSchemes("OpenIdConnect")
              .RequireAuthenticatedUser());

    options.AddPolicy("ServiceOnly", policy =>
        policy.AddAuthenticationSchemes("Bearer")
              .RequireAuthenticatedUser());
});


builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL().RequireAuthorization();

//app.MapGet("/", async context =>
//{
//    if (!context.User.Identity?.IsAuthenticated ?? true)
//    {
//        // If not authenticated → challenge (redirect to Microsoft login popup)
//        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
//        return;
//    }

//    // If authenticated → show user info
//    var name = context.User.Identity?.Name ?? "Unknown";
//    await context.Response.WriteAsync($"Hello, {name}. You are logged in!");
//});

// built-in enforcement

//app.Use(async (context, next) =>
//{
//    if (!context.User.Identity?.IsAuthenticated ?? true)
//    {
//        // Challenge for login
//        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
//        return;
//    }
//    await next();
//});


app.Run();
