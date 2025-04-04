using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository;
using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Service;
using AntiSwearingChatBox.Service.Interface;
using AntiSwearingChatBox.AI.Filter;
using AntiSwearingChatBox.AI.Services;
using AntiSwearingChatBox.Server.Service;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using AntiSwearingChatBox.Server.Middleware;
using Microsoft.AspNetCore.SignalR;
using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers with JSON options
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Add SignalR services
            services.AddSignalR();

            // Register database context
            services.AddDbContext<AntiSwearingChatBoxContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AntiSwearingChatBox"));
            });

            // Add Gemini AI Services
            services.Configure<GeminiSettings>(options =>
            {
                options.ApiKey = Configuration["GeminiSettings:ApiKey"] ?? "AIzaSyAr-Vto1YywEwssTDzeEmkS2P4caVaU13o";
                options.ModelName = Configuration["GeminiSettings:ModelName"] ?? "gemini-2.0-flash-lite";
            });
            services.AddSingleton<GeminiService>();

            // Register repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddScoped<IChatThreadService, ChatThreadService>();
            services.AddScoped<IMessageHistoryService, MessageHistoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserWarningService, UserWarningService>();
            services.AddScoped<IThreadParticipantService, ThreadParticipantService>();
            services.AddScoped<IFilteredWordService, FilteredWordService>();
            services.AddScoped<IAuthService, AuthService>();

            // Register profanity filter service with AI capabilities
            services.AddSingleton<IProfanityFilter, ProfanityFilterService>(sp => 
            {
                var geminiService = sp.GetRequiredService<GeminiService>();
                return new ProfanityFilterService(geminiService);
            });

            // Configure JWT
            var jwtSettings = Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JWT SecretKey is not configured"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Configure CORS to allow CLI client access
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Configure Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AntiSwearing ChatBox API", Version = "v1" });
                
                // Add JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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
                        new List<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use request/response logging middleware
            app.UseRequestResponseLogging();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowLocalhost");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<Hubs.ChatHub>("/chatHub");
            });
        }
    }
} 