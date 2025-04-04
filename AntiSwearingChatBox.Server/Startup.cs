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
using AntiSwearingChatBox.Server.AI;

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
            // Get logger from service provider
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
        
            // Add controllers
            services.AddControllers();

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
            logger.LogDebug("Configuring JWT authentication...");
            
            string? secretKey = null;
            string? issuer = null;
            string? audience = null;
            
            // Try to get JWT settings from JwtSettings section
            var jwtSettings = Configuration.GetSection("JwtSettings");
            secretKey = jwtSettings["SecretKey"];
            issuer = jwtSettings["Issuer"];
            audience = jwtSettings["Audience"];
            
            // If not found, try the alternative JWT section
            if (string.IsNullOrEmpty(secretKey))
            {
                logger.LogDebug("JWT settings not found in JwtSettings section, trying JWT section...");
                var jwtSection = Configuration.GetSection("JWT");
                secretKey = jwtSection["SecretKey"];
                issuer = jwtSection["ValidIssuer"];
                audience = jwtSection["ValidAudience"];
            }
            
            // Only log if secret key is missing, not the actual configuration values
            if (string.IsNullOrEmpty(secretKey))
            {
                logger.LogWarning("JWT SecretKey is not configured properly");
            }
            
            // Ensure we have the required JWT settings
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured in either JwtSettings or JWT section");
            }
            
            var key = Encoding.ASCII.GetBytes(secretKey);
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = !string.IsNullOrEmpty(issuer),
                        ValidIssuer = issuer ?? "AntiSwearingChatBox",
                        ValidateAudience = !string.IsNullOrEmpty(audience),
                        ValidAudience = audience ?? "AntiSwearingChatBox",
                        ClockSkew = TimeSpan.Zero
                    };
                    
                    // Configure SignalR authentication
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            
                            // If the request is for our chat hub
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
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