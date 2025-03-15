using Luxa.Data;
using Luxa.Interfaces;
using Luxa.MappingProfiles;
using Luxa.Models;
using Luxa.Repository;
using Luxa.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Luxa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //��czenie si� z baz� danych
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                //appsettings.json zawiera w sobie connection string skopiowany z w�a�ciwo�ci z baz danych a "LuxaDb" to odwo�anie do appsettings.json
                options.UseSqlServer(builder.Configuration.GetConnectionString("LuxaDb"));
            });
            builder.Services.AddIdentity<UserModel, IdentityRole>(options =>
            {
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Home/Landing";
            });
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //Repositories
            builder.Services.AddScoped<IContactRepository, ContactRepository>();
            builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IFollowRepository, FollowRepository>();

            //Services
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IContactService, ContactService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IFollowService, FollowService>();

            //Mapper
            builder.Services.AddAutoMapper(typeof(UserProfile));



            // Google
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("GoogleKeys");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Home/Landing");
                        return Task.CompletedTask;
                    };

                });

            var app = builder.Build();

            NotificationsDataInit.SeedNotifications(app);
            _ = IdentityDataInit.SeedUsersAndRolesAsync(app);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //Routing do logowania
            app.MapControllerRoute(
                name: "SignIn",
                pattern: "signin",
                defaults: new { controller = "Account", action = "SignIn" });
            //Routing do Rejestracji
            app.MapControllerRoute(
                name: "SignUp",
                pattern: "signup",
                defaults: new { controller = "Account", action = "SignUp" });
            app.MapControllerRoute(
                name: "UserNotifications",
                pattern: "Account/UserNotifications/{notificationId}",
                defaults: new { controller = "Account", action = "UserNotifications" });
            app.Run();
        }
    }
}
