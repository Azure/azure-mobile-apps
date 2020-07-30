using Azure.Mobile.Server.Test.E2EServer.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Azure.Mobile.Server.Test.E2EServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            WebHostEnvironment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var _secret = Configuration.GetSection("JwtConfig").GetSection("Secret").Value;
            var _expDate = Configuration.GetSection("JwtConfig").GetSection("ExpirationInMinutes").Value;

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secret)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = "localhost",
                        ValidAudience = "localhost"
                    };
                });

            services.AddDbContext<E2EDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("MS_TableConnectionString");
                if (connectionString.Contains(":memory:"))
                {
                    options.UseSqlite(SQLiteHelper.GetConnection(connectionString));
                }
                else
                {
                    options.UseSqlServer(connectionString);
                }
                
            });

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddAzureMobileApps();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableAzureMobileApps();
            });
        }
    }
}
