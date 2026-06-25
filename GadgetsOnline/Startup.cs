
using System;
using System.IO;
using GadgetsOnline.Data;
using GadgetsOnline.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GadgetsOnline
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationManager.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews();

            // ADO.NET data layer. The Database helper owns the connection string;
            // repositories wrap the stored procedures. The password is supplied
            // separately (user secrets / environment) and merged in, so it is not
            // stored in appsettings.json. The database name stays visible there.
            var baseConnectionString = Configuration.GetConnectionString("GadgetsOnlineEntities");
            var dbPassword = Configuration["DbPassword"];
            var connectionString = Database.BuildConnectionString(baseConnectionString, dbPassword);
            services.AddSingleton(new Database(connectionString));
            services.AddScoped<ProductRepository>();
            services.AddScoped<CategoryRepository>();
            services.AddScoped<CartRepository>();
            services.AddScoped<OrderRepository>();

            services.AddScoped<IInventory, Inventory>();
            services.AddScoped<IShoppingCart, ShoppingCart>();
            services.AddScoped<IOrderProcessing, OrderProcessing>();
            //Added Services
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Bootstrap the database from SQL scripts (schema, procs, seed).
            var database = app.ApplicationServices.GetRequiredService<Database>();
            var scriptsDirectory = Path.Combine(env.ContentRootPath, "Database");
            database.Initialize(scriptsDirectory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //Added Middleware

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; }
    }

}
