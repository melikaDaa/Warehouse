using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Domain.Entities;
using Warehouse.Api.Infrastructure.Persistence;
using Warehouse.Api.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------- DbContext ----------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Repositories + UnitOfWork
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// ---------- Identity ----------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
});

// ---------- JWT ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
// ÈÑÇ? ÓÇÏå ÔÏä ÊãÑ?ä¡ ÝÚáÇð Issuer/Audience ÑÇ æá?Ï?Ê äã?˜ä?ã
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            // ÈÑÇ? ÊãÑ?ä: ÝÚáÇð Issuer æ Audience ÑÇ ˜ ä˜ä
            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateLifetime = true
        };
    });

// ---------- Authorization + Policies ----------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("SystemAdmin"));

    options.AddPolicy("CanManageProducts", policy =>
        policy.RequireRole("SystemAdmin", "WarehouseManager"));

    options.AddPolicy("CanDoStockMovements", policy =>
        policy.RequireRole("SystemAdmin", "WarehouseManager", "NormalUser"));

    options.AddPolicy("CanViewReports", policy =>
        policy.RequireRole("SystemAdmin", "WarehouseManager", "Auditor"));
});

// ---------- Controllers ----------
builder.Services.AddControllers();

// ---------- Swagger + JWT ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Warehouse API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Just paste the JWT token here, WITHOUT 'Bearer ' prefix"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Migration + Seed ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(app.Services);
}

// ---------- Middleware ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
