using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OctopusSamples.OctoPetShop.Controllers.Client;

namespace OctopusSamples.OctoPetShop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<AppSettings>(Configuration);
            services.AddScoped<IProductClient, ProductClient>();

            services.AddMvcCore(options =>
            {
                options.RequireHttpsPermanent = true; //does not affect API requests
                options.RespectBrowserAcceptHeader = true; //false by default
            })
            .AddApiExplorer()
            .AddFormatterMappings()
            .AddNewtonsoftJson()
            .AddCacheTagHelper()
            .AddDataAnnotations()
            .AddAuthorization()
            .AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthorization();

            /* 
            Check to see if we're running in a container.  This is only for this test application
            do not do something like this in Production
            */
            if (System.Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == null)
                app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
