using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GymdataOnline
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IDelegationRepository, DelegationRepository>();
            services.AddScoped<IFunctionRepository, FunctionRepository>();
            services.AddScoped<DbContext, AccreditationDbContext>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDistributedMemoryCache();
            services.AddScoped<IUserDependencyRepository, UserDependencyRepository>();
            services.AddScoped<IRoleDependencyRepository, RoleDependencyRepository>();
            services.AddScoped<IProvisionalConfirmRepository, ProvisionalConfirmRepository>();
            services.AddScoped<IDefinitiveConfirmRepository, DefinitiveConfirmRepository>();
            services.AddScoped<IInterestRepository, InterestRepository>();
            services.AddTransient<IPhotoService, FilePhotoService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IInvoiceGenerator, DefaultInvoiceGenerator>();
            services.AddTransient<IEventMailRepository, EventMailRepository>();
            services.AddDbContext<AccreditationDbContext>(options =>
            options.UseSqlServer(Configuration["DbConnection:AccreditationDbConnection"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
                ,
                Secure = CookieSecurePolicy.None
            });
            //for the first version,support only aze lang
            var supportedCultures = new[]
                {
                    new CultureInfo("az-Latn-AZ")
                };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(supportedCultures[0]),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });
        }
    }
}
