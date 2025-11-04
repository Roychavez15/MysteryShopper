using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MysteryShopper.API.Data;
using MysteryShopper.API.Domain.Identity;
using MysteryShopper.API.Infrastructure;
using MysteryShopper.API.Infrastructure.Files;
using MysteryShopper.API.Infrastructure.Mapping;
using MysteryShopper.API.Services;
using System;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Db
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

// Current user service + HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Auth
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = key,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Repositories & UoW
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// File Storage
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mystery Shopper API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer token"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed roles and admin
using (var scope = app.Services.CreateScope())
{
    //var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    //var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    //string[] roles = [Roles.Admin, Roles.Client, Roles.Evaluator];
    //foreach (var r in roles)
    //{
    //    if (!await roleMgr.RoleExistsAsync(r))
    //        await roleMgr.CreateAsync(new IdentityRole(r));
    //}

    //var adminEmail = "admin@ms.local";
    //var admin = await userMgr.FindByEmailAsync(adminEmail);
    //if (admin is null)
    //{
    //    admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
    //    await userMgr.CreateAsync(admin, "Admin#12345");
    //    await userMgr.AddToRoleAsync(admin, Roles.Admin);
    //}
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    await DbInitializer.SeedAsync(services, logger);
}

app.UseStaticFiles(); // for wwwroot uploads
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();