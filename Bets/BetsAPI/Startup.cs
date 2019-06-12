using BetsAPI.EventQueue;
using BetsData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BetsAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer(
                    "Bearer",
                    options =>
                    {
                        options.Authority = "http://localhost:20080";
                        options.RequireHttpsMetadata = false;
                        options.MetadataAddress = "http://leagueofbets_identity/.well-known/openid-configuration";

                        options.Audience = "bets";
                    });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<BetsDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IBetEventProducer, RabbitMqBetEventProducer>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}