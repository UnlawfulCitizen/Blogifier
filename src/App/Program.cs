﻿using Core;
using Core.Data;
using Core.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                try
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                }
                catch { }
                
                var userMgr = (UserManager<AppUser>)services.GetRequiredService(typeof(UserManager<AppUser>));
                if (!userMgr.Users.Any())
                {
                    userMgr.CreateAsync(new AppUser { UserName = "admin", Email = "admin@us.com" }, "Admin@pass1");
                    userMgr.CreateAsync(new AppUser { UserName = "demo", Email = "demo@us.com" }, "Demo@pass1");
                }

                // load application settings from appsettings.json
                var app = services.GetRequiredService<IAppService<AppItem>>();
                AppConfig.SetSettings(app.Value);

                if (!context.BlogPosts.Any())
                {
                    services.GetRequiredService<IStorageService>().Reset();
                    AppData.Seed(context);
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
    }
}