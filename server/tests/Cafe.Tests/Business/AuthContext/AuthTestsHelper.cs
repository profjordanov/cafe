﻿using Cafe.Api.Configuration;
using Cafe.Core.AuthContext.Commands;
using Cafe.Domain.Entities;
using Cafe.Domain.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cafe.Tests.Business.AuthContext
{
    public class AuthTestsHelper
    {
        private readonly AppFixture _fixture;

        public AuthTestsHelper(AppFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task AddClaimsAsync(Guid userId, IEnumerable<Claim> claims)
        {
            await _fixture.ExecuteScopeAsync(async sp =>
            {
                var userManager = sp.GetRequiredService<UserManager<User>>();

                var user = await userManager.FindByIdAsync(userId.ToString());

                if (user != null)
                {
                    await userManager.AddClaimsAsync(user, claims);
                }
            });
        }

        public async Task LoginAndCheckClaim(string email, string password, Func<Claim, bool> claimPredicate)
        {
            var loginResult = await _fixture.SendAsync(new Login
            {
                Email = email,
                Password = password
            });

            loginResult.Exists(jwt =>
            {
                var decoded = new JwtSecurityToken(jwt.TokenString);

                return decoded
                    .Claims
                    .Any(claimPredicate);
            })
            .ShouldBeTrue();
        }

        public async Task<JwtView> Login(string email, string password) =>
            (await _fixture.SendAsync(new Login
            {
                Email = email,
                Password = password
            }))
            .ValueOr(() => throw new InvalidOperationException("Tried to login with invalid credentials."));

        public async Task<JwtView> RegisterAndLogin(Register command)
        {
            await Register(command);
            return await Login(command.Email, command.Password);
        }

        public Task Register(Register command) =>
            _fixture.SendAsync(command);

        public Task<(string Email, string Password)> RegisterAdminAccountIfNotExisting() =>
            _fixture.ExecuteScopeAsync(async serviceProvider =>
            {
                var userManager = serviceProvider.GetService<UserManager<User>>();
                var configuration = serviceProvider.GetService<IConfiguration>();

                return (await DatabaseConfiguration
                    .AddDefaultAdminAccountIfNoneExisting(userManager, configuration))
                    .ValueOr(configuration.GetAdminCredentials());
            });

        public async Task<string> GetAdminToken()
        {
            var (Email, Password) = await RegisterAdminAccountIfNotExisting();

            var token = (await Login(Email, Password))
                .TokenString;

            return token;
        }
    }
}
