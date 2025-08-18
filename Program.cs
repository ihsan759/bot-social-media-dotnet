using System.Text.Json;
using bot_social_media.Services;
using bot_social_media.Utils;
using BotSocialMedia.Data;
using BotSocialMedia.Repositories;
using BotSocialMedia.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BotSocialMedia.Services;
using Microsoft.AspNetCore.Authorization;

// Erase default claim mapping
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var config = builder.Configuration;

builder.Services.AddScoped<JwtHandling>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<BotService>();

// Tambahkan Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:AccessSecret"]!)
            ),
            ClockSkew = TimeSpan.Zero,

            // Set the role claim type
            RoleClaimType = "role",
        };

        // customize the events to handle token validation errors
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claims = context.Principal?.Claims;

                var idClaim = claims?.FirstOrDefault(c => c.Type == "id")?.Value;
                var roleClaim = claims?.FirstOrDefault(c => c.Type == "role")?.Value;

                // Pastikan id ada dan valid Guid
                if (string.IsNullOrWhiteSpace(idClaim) || !Guid.TryParse(idClaim, out _))
                {
                    throw new HttpException("Invalid or missing 'id' claim", StatusCodes.Status401Unauthorized);
                }

                // Pastikan role ada
                if (string.IsNullOrWhiteSpace(roleClaim))
                {
                    throw new HttpException("Invalid or missing 'role' claim", StatusCodes.Status401Unauthorized);
                }

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // turn off the default response
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                throw new HttpException(
                    message: "Access token is missing or invalid",
                    statusCode: StatusCodes.Status401Unauthorized
                );
            },
            OnForbidden = context =>
            {
                throw new HttpException(
                    message: "You do not have permission to access this resource",
                    statusCode: StatusCodes.Status403Forbidden
                );
            },
            OnAuthenticationFailed = context =>
            {
                var ex = context.Exception;

                if (ex is SecurityTokenExpiredException ||
                    ex is SecurityTokenInvalidLifetimeException ||
                    ex.Message.Contains("expired", StringComparison.OrdinalIgnoreCase))
                {
                    throw new HttpException(
                        message: "Token expired",
                        statusCode: StatusCodes.Status401Unauthorized
                    );
                }

                throw new HttpException(
                    message: "Invalid token",
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VerifiedOnly", policy =>
        policy.Requirements.Add(new VerifiedRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, VerifiedHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors?.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            status = 400,
            message = "Validation failed",
            errors
        };

        return new BadRequestObjectResult(response);
    };
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddSingleton<CloudinaryService>();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IBotRepository, BotRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// configure custom middleware for error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

app.Use(async (context, next) =>
{
    await next();

    if (!context.Response.HasStarted)
    {
        if (context.Response.StatusCode == 404)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = 404,
                message = "Page Not Found"
            }));
        }
        else if (context.Response.StatusCode == 405)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = 405,
                message = "Method Not Allowed"
            }));
        }
    }
});




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
