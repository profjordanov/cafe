﻿using Cafe.Api.OperationFilters;
using Cafe.Business;
using Cafe.Business.AuthContext;
using Cafe.Core;
using Cafe.Core.AuthContext;
using Cafe.Core.AuthContext.Configuration;
using Cafe.Domain.Entities;
using Cafe.Domain.Events;
using Cafe.Domain.Views;
using Cafe.Persistance.EntityFramework;
using Marten;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafe.Api.Configuration
{
    public static class DependenciesConfiguration
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            services.AddTransient<IMenuItemsService, MenuItemsService>();
        }

        public static void AddCqrs(this IServiceCollection services)
        {
            services.AddScoped<IEventBus, EventBus>();
        }

        public static void AddDbContext(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseNpgsql(connectionString));
        }

        public static void AddJwtIdentity(this IServiceCollection services, IConfigurationSection jwtConfiguration, Action<AuthorizationOptions> config)
        {
            services.AddTransient<IJwtFactory, JwtFactory>();

            services.AddIdentity<User, IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            var signingKey = new SymmetricSecurityKey(
                Encoding.Default.GetBytes(jwtConfiguration["Secret"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtConfiguration[nameof(JwtConfiguration.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtConfiguration[nameof(JwtConfiguration.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.Configure<JwtConfiguration>(options =>
            {
                options.Issuer = jwtConfiguration[nameof(JwtConfiguration.Issuer)];
                options.Audience = jwtConfiguration[nameof(JwtConfiguration.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtConfiguration[nameof(JwtConfiguration.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;

                configureOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config(options);
            });
        }

        public static void AddMarten(this IServiceCollection services, IConfiguration configuration)
        {
            var documentStore = DocumentStore.For(options =>
            {
                var config = configuration.GetSection("EventStore");
                var connectionString = config.GetValue<string>("ConnectionString");
                var schemaName = config.GetValue<string>("Schema");

                options.Connection(connectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.DatabaseSchemaName = schemaName;
                options.DatabaseSchemaName = schemaName;

                options.Events.InlineProjections.AggregateStreamsWith<Tab>();
                options.Events.InlineProjections.Add(new TabViewProjection());

                var events = typeof(TabOpened)
                    .Assembly
                    .GetTypes()
                    .Where(t => typeof(IEvent).IsAssignableFrom(t))
                    .ToList();

                options.Events.AddEventTypes(events);
            });

            services.AddSingleton<IDocumentStore>(documentStore);

            services.AddScoped(sp => sp.GetService<IDocumentStore>().OpenSession());
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(setup =>
            {
                var documentationPath = Path.Combine(AppContext.BaseDirectory, "Cafe.Api.Documentation.xml");

                if (File.Exists(documentationPath))
                {
                    setup.SwaggerDoc("v1", new Info { Title = "Cafe.Api", Version = "v1" });
                    setup.IncludeXmlComments(documentationPath);

                    setup.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Enter 'Bearer {token}' (don't forget to add 'bearer') into the field below.", Name = "Authorization", Type = "apiKey" });

                    setup.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {
                        { "Bearer", Enumerable.Empty<string>() },
                    });

                    setup.OperationFilter<OptionOperationFilter>();
                }
            });
        }
    }
}