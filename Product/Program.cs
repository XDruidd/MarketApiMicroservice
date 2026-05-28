using System.Text;
using Asp.Versioning;
using Market.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Product.Data;
using Product.Services;
using Product.Services.Inteface;
using DotNetEnv;
using System.Text.Json;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var secretsToLoad = new[] { "prod/Market/product/s3", "prod/Market/jwt" };

var client = new AmazonSecretsManagerClient(RegionEndpoint.EUNorth1); 

foreach (var secretName in secretsToLoad)
{
    try
    {
        var request = new GetSecretValueRequest { SecretId = secretName };
        var response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(response.SecretString))
        {
            var secretData = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

            if (secretData != null)
            {
                builder.Configuration.AddInMemoryCollection(secretData!);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        throw;
    }
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;

    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("x-version") 
    );
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3Service>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            
            RoleClaimType = "role"   
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ProductDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    { 
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "migration error");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();