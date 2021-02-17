using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Utilities
{
    public static class DataManage
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }
        public static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(":");

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IHost host)
        {
            using var svcScope = host.Services.CreateScope();
            var svcProvider = svcScope.ServiceProvider;

            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            // an instance of role manager
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
            // an instance of usermanager
            var userManageSvc = svcProvider.GetRequiredService<UserManager<CustomUser>>();

            await dbContextSvc.Database.MigrateAsync();
            //await dbContextSvc.Database.MigrateAsync();

            // add role to the system
            await SeedRoleAsync(roleManagerSvc);
            //add user
            await SeedUserAsync(userManageSvc);
            // assign role
            await AssignRoleAsync(userManageSvc);

        }

        private static async Task SeedRoleAsync(RoleManager<IdentityRole> roleSvc)
        {
            // call upon the roleSvc to add the new role
            await roleSvc.CreateAsync(new IdentityRole("Administrator"));
        }
        private static async Task SeedUserAsync(UserManager<CustomUser> userManagerSvc)
        {
            //create your self as a user

            var adminUser = new CustomUser()
            {
                Email = "arthastheking113@gmail.com",
                UserName = "arthastheking113@gmail.com",
                FirstName = "Lan",
                LastName = "Le",
                PhoneNumber = "4023040329",
                EmailConfirmed = true

            };
            await userManagerSvc.CreateAsync(adminUser, "Nhoclanro1!");
            //create someone else as a moderator

        }
        private static async Task AssignRoleAsync(UserManager<CustomUser> userManagerSvc)
        {
            // get a reference to the admin user
            var adminUser = await userManagerSvc.FindByEmailAsync("arthastheking113@gmail.com");
            await userManagerSvc.AddToRoleAsync(adminUser, "Administrator");
        }
    }
}
