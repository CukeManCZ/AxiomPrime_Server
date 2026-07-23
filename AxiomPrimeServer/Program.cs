using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key!)
            ),

            ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                Console.WriteLine("HEADER TOKEN:");
                Console.WriteLine(ctx.Request.Headers["Authorization"]);
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT FAILED:");
                Console.WriteLine(ctx.Exception);
                return Task.CompletedTask;
            },

            OnTokenValidated = ctx =>
            {
                Console.WriteLine("TOKEN VALID");
                return Task.CompletedTask;
            }
        };
    });
    

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<EventBus>();
builder.Services.AddSingleton<PlayerLockProvider>();

// Repositories
builder.Services.AddScoped<ExperienceRepository>();
builder.Services.AddScoped<CurrenciesRepository>();
builder.Services.AddScoped<EnergyRepository>();
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddScoped<InventoryRepository>();
builder.Services.AddScoped<ShipInventoryRepository>();

// Services (Business logic)
builder.Services.AddScoped<GlobalPlayerDataService>();
builder.Services.AddScoped<BrainCloudService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IShipInventoryService, ShipInventoryService>();

// API Layers
builder.Services.AddScoped<GlobalPlayerDataAPI>();
builder.Services.AddScoped<InventoryAPI>();
builder.Services.AddScoped<ShipInventoryAPI>();

// HTTP Clients
builder.Services.AddHttpClient<BrainCloudClient>();

// Options
builder.Services.Configure<BrainCloudOptions>(
    builder.Configuration.GetSection("BrainCloud"));

var app = builder.Build();

app.MapGet("/", (IEnumerable<EndpointDataSource> sources) =>
{
    var endpoints = sources.SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e =>
        {
            var method = e.Metadata
                .OfType<HttpMethodMetadata>()
                .FirstOrDefault()?.HttpMethods.FirstOrDefault();

            var route = e.RoutePattern.RawText;

            var controller = e.DisplayName?
                .Split('.')
                .FirstOrDefault();

            return $"{controller} -> {method} {route}";
        });

    return Results.Text(string.Join("\n", endpoints));
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();