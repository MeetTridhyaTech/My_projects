//using Microsoft.EntityFrameworkCore;
//using StudentCorewebAPI_Project.Data;
//using FluentValidation;
//using FluentValidation.AspNetCore;
//using StudentCorewebAPI_Project.Validators;
//using Serilog;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Builder;
//using StudentCorewebAPI_Project.Repository;
//using AutoMapper;
//using System.Reflection;
//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using StudentCorewebAPI_Project.Services;
//using Microsoft.OpenApi.Models;
//using StudentCorewebAPI_Project.Repository_Interface;
//using System.Security.Claims;
//using StudentCorewebAPI_Project.SignalR;
////using static StudentCorewebAPI_Project.Repository.MenuRepository;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddControllers();
////Defines Cors Policy

//var jwtSetting = builder.Configuration.GetSection("JwtSettings");
//var key = Encoding.UTF8.GetBytes(jwtSetting["Key"]);
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
//            ValidAudience = builder.Configuration["JwtSettings:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
//            RoleClaimType= ClaimTypes.Role
//        };
//        options.Events = new JwtBearerEvents
//        {
//            //OnForbidden = async context =>
//            //{
//            //    context.Response.StatusCode = 403;
//            //    context.Response.ContentType = "application/json";
//            //    await context.Response.WriteAsync("{\"message\": \"Restricted By Admin\"}");
//            //}
//        };
//    });

//builder.Services.AddAuthorization();

//// Add Swagger with JWT Authentication
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });

//    // Add JWT Authentication to Swagger
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter the JWT token in the format Bearer."
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console()
//    .WriteTo.File("Logs/api_log.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// FluentValidation
//builder.Services.AddValidatorsFromAssemblyContaining<UserValidators>();
//builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidators>();
//builder.Services.AddFluentValidationAutoValidation();

//// AutoMapper
//builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
//// Common pagination method
//builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
//// Email Service
//builder.Services.AddScoped<IEmailService, EmailService>();
//// Repository
//builder.Services.AddScoped<IUserRepository, UserRepository>();
////Permission Repository
//builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
////Role repository
//builder.Services.AddScoped<IRoleRepository, RoleRepository>();
////Menu repository
//builder.Services.AddScoped<IMenuRepository, MenuRepository>();

//// SignalR Chat
//builder.Services.AddSignalR();
//builder.Services.AddScoped<IChatRepository, ChatRepository>();

//// Database Context
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("StudentCorewebAPI_Project")));

//// JWT Service
//builder.Services.AddScoped<JwtService>();


////var corsPolicy = "AllowAll"; // Ensure consistent policy naming

////builder.Services.AddCors(options =>
////{
////    options.AddPolicy(corsPolicy, policy =>
////    {
////        policy.AllowAnyOrigin()
////              .AllowAnyHeader()
////              .AllowAnyMethod()
////              .SetIsOriginAllowed(_ => true); ;
////    });
////});

//var corsPolicy = "AllowAll";

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(corsPolicy, policy =>
//    {
//        policy
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials()
//            .SetIsOriginAllowed(_ => true); // ✅ This replaces AllowAnyOrigin()
//    });
//});


//var app = builder.Build();

//app.UseCors(corsPolicy);

//// Middleware setup
//app.UseAuthentication();    
//app.UseAuthorization();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseSerilogRequestLogging();
////app.UseHttpsRedirection();
////app.UseAuthorization();
//app.MapControllers();

//// SignalR endpoint 
//app.MapHub<ChatHub>("/chathub");

//app.Run();

using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using StudentCorewebAPI_Project.Validators;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
using StudentCorewebAPI_Project.Repository;
using StudentCorewebAPI_Project.Repository_Interface;
using StudentCorewebAPI_Project.Services;
using StudentCorewebAPI_Project.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/api_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token in the input below.\nExample: Bearer abcdef12345"
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

//Load JWT config
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// JWT Authentication with SignalR support
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        // Enable SignalR JWT auth from access_token query param
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Use token from query only for SignalR
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS with AllowCredentials (required for WebSocket)
var corsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            //.WithOrigins("http://172.16.3.84:8091")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StudentCorewebAPI_Project")));

//  AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//  FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserValidators>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidators>();
builder.Services.AddFluentValidationAutoValidation();

// DI - Services & Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

var app = builder.Build();

app.UseRouting();


// Enable middleware
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.Run();
