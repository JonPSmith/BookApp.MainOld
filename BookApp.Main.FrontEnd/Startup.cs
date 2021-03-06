// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using BookApp.Books.AppSetup;
using BookApp.Books.Infrastructure.CachedValues.ConcurrencyHandlers;
using BookApp.Books.Infrastructure.CachedValues.EventHandlers;
using BookApp.Books.Persistence;
using BookApp.Main.FrontEnd.Logger;
using GenericEventRunner.ForSetup;
using GenericServices.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftDeleteServices.Configuration;

namespace BookApp.Main.FrontEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) //#A
        {
            services.AddControllersWithViews() //#B
                //.AddRazorRuntimeCompilation() //This recompile a razor page if you edit it while the app is running
                //Added this because my logs display needs the enum as a string
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });


            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            //This registers both DbContext. Each MUST have a unique MigrationsHistoryTable for Migrations to work
            services.AddDbContext<BookDbContext>( 
                options => options.UseSqlServer(connectionString, dbOptions =>
                dbOptions.MigrationsHistoryTable("BookMigrationHistoryName")));

            services.AddHttpContextAccessor();

            //ModMod.Books startup
            var test = services.RegisterBooksServices(Configuration);

            //Register EfCore.GenericEventRunner
            var eventConfig = new GenericEventRunnerConfig();
            eventConfig.RegisterSaveChangesExceptionHandler<BookDbContext>(BookWithEventsConcurrencyHandler.HandleCacheValuesConcurrency);
            eventConfig.AddActionToRunAfterDetectChanges<BookDbContext>(BookDetectChangesExtensions.ChangeChecker);
            var logs = services.RegisterGenericEventRunner(eventConfig,
                Assembly.GetAssembly(typeof(ReviewAddedHandler))   //SQL cached values event handlers
            );

            //Register EfCoreGenericServices
            services.ConfigureGenericServicesEntities(typeof(BookDbContext))
                .ScanAssemblesForDtos(
                    BooksStartupInfo.GenericServiceAssemblies.ToArray()
                ).RegisterGenericServices();

            var softLogs = services.RegisterSoftDelServicesAndYourConfigurations();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            loggerFactory.AddProvider(new RequestTransientLogger(() => httpContextAccessor));
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}