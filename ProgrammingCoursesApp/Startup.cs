using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProgrammingCoursesApp.Data;
using System;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(opts =>
            {
                opts.SignIn.RequireConfirmedEmail = false;
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            CreateRoles(provider).GetAwaiter().GetResult();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var rolesManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var usersManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "CourseCreator", "User" };

            IdentityResult result;

            foreach (var role in roles)
            {
                var roleExists = await rolesManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    result = await rolesManager.CreateAsync(new IdentityRole(role));
                }
            }

            var findAdmin = await usersManager.FindByEmailAsync(Configuration["AdminUser:Email"]);

            if (findAdmin == null)
            {
                var admin = new IdentityUser
                {
                    UserName = Configuration["AdminUser:UserName"],
                    Email = Configuration["AdminUser:Email"],
                };
                var adminCreation = await usersManager.CreateAsync(admin, Configuration["AdminUser:Password"]);
                if (adminCreation.Succeeded)
                {
                    await usersManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

    }
}
