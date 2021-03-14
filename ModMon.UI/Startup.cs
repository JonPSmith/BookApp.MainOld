// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using System.Text.Json.Serialization;
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
using ModMon.Books.Infrastructure.CachedValues;
using ModMon.Books.Infrastructure.CachedValues.ConcurrencyHandlers;
using ModMon.Books.Infrastructure.CachedValues.EventHandlers;
using ModMon.Books.Infrastructure.Seeding;
using ModMon.Books.Persistence;
using ModMon.Books.ServiceLayer.Cached;
using ModMon.Books.ServiceLayer.Common.Dtos;
using ModMon.Books.ServiceLayer.GoodLinq;
using ModMon.Books.ServiceLayer.GoodLinq.Dtos;
using ModMon.Books.ServiceLayer.Udfs;
using MonMon.UI.Logger;
using NetCore.AutoRegisterDi;
using SoftDeleteServices.Configuration;

namespace MonMon.UI
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


            //This registers all the services across all the projects in this application
            var diLogs = services.RegisterAssemblyPublicNonGenericClasses(
                    Assembly.GetAssembly(typeof(ICheckFixCacheValuesService)),
                    Assembly.GetAssembly(typeof(BookListDto)),
                    Assembly.GetAssembly(typeof(IBookGenerator)),
                    Assembly.GetAssembly(typeof(IListBooksCachedService)),
                    Assembly.GetAssembly(typeof(IListBooksService)),
                    Assembly.GetAssembly(typeof(IListUdfsBooksService))
                )
                .AsPublicImplementedInterfaces();

            //Register EfCore.GenericEventRunner
            var eventConfig = new GenericEventRunnerConfig
            {};
            eventConfig.RegisterSaveChangesExceptionHandler<BookDbContext>(BookWithEventsConcurrencyHandler.HandleCacheValuesConcurrency);
            eventConfig.AddActionToRunAfterDetectChanges<BookDbContext>(BookDetectChangesExtensions.ChangeChecker);
            var logs = services.RegisterGenericEventRunner(eventConfig,
                Assembly.GetAssembly(typeof(ReviewAddedHandler))   //SQL cached values event handlers
            );

            //Register EfCoreGenericServices
            services.ConfigureGenericServicesEntities(typeof(BookDbContext))
                .ScanAssemblesForDtos(
                    Assembly.GetAssembly(typeof(BookListDto)),
                    Assembly.GetAssembly(typeof(AddReviewDto))
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