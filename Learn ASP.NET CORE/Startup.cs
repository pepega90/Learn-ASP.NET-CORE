using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Learn_ASP.NET_CORE.Models;
using Learn_ASP.NET_CORE.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Google;

namespace Learn_ASP.NET_CORE
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        #region ConfigureServicesMethod
        public void ConfigureServices(IServiceCollection services)
        {
            // ConfigureServices(), mengkonfigurasi layanan yang diperlukan oleh aplikasi
            services.AddDbContextPool<AppDbContext>( // <= connect to database, menggunakan sql server
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));
            // object services, memiliki method AddMvc(), yang berguna untuk memberikan MVC service ke project kita

            /* ASP.NET Core menyediakan 3 metode berikut untuk mendaftarkan layanan dengan dependency injection container.
             * 1. AddSingleton(), hanya ada 1 instance ketika service pertama kali diminta dan instance itu digunakan oleh semua permintaan (request) http di seluruh aplikasi
             * 2. AddTransient(), instance baru dibuat setiap kali instance diminta, apakah itu dari dalam lingkup permintaan http yang sama atau berbeda
             * 3. AddScoped(), kita dapat instance yang sama dengan lingkup permintaan http yang diberikan (sama), tetapi instance baru di seluruh permintaan http yang berbeda
             */
            services.AddMvc(options =>
            {
                // Membuat Global Authorization
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.EnableEndpointRouting = false;
            }).AddXmlSerializerFormatters();

            // Register IEmployeeRepository dependency
            services.AddScoped<IEmployeeRepository, SqlEmployeeRepository>();

            // Adding ASP.NET Core Identity Services
            // IdentityUser, class bawaan untuk mengatur role
            // IdentityRole, class bawaan asp net untuk mengatur role
            // AddEntityFrameworkStores(), method untuk menyimpan data dari user dan role
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                // Email harus dikonfirmasi agar bisa login
                options.SignIn.RequireConfirmedEmail = true;
                // Kita meng-add custom provider
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                // Mengatur lockout akun
                // MaxFailedAccessAttempts properti, memberikan maksimal user untuk mencoba login
                options.Lockout.MaxFailedAccessAttempts = 5;
                // DefaultLockoutTimeSpan properti, maksimal batas waktu setelah user sudah mencoba untuk login sebanyak MaxFailedAccessAttempts
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);// <= nilai defaultnya adalah 5 menit

            }).AddEntityFrameworkStores<AppDbContext>()
            // AddDefaultTokenProviders(), untuk membuat token (untuk konfirmasi email)
              .AddDefaultTokenProviders()
              .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");

            // Kita bisa mengoverride dengan bantuan Configure() method atau langsung saat kita Menambahkan 
            // Identity service
            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequiredLength = 5;
            //    options.Password.RequiredUniqueChars = 3;
            //});

            // Implementasi Policy Authorize
            services.AddAuthorization(options =>
            {
                // Step untuk membuat authorize claim based authorization
                // 1. Membuat Claims Policy
                // 2. Kita bisa gunakan policy tersebut di controller atau pun di action controller
                // Jika kita melakukan chaining policy method, misal
                // RequireClaim().RequireRole() = ini adalah AND (&&) Relationship
                // Untuk membuat OR (||) Relationship, kita bisa gunakan RequireAssertion() 
                options.AddPolicy("DeleteRolePolicy",
                        policy => policy.RequireClaim("Delete Role"));
                // Claim type, case-insensitive
                // Claim value, case-sensitive
                options.AddPolicy("EditRolePolicy",
                        // membuat custom authorization policy dengan func,
                        // RequireAssertion(), menerima parameter Func

                        // di bawah ini kita me-register custom authorization requirement
                        policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

                // Membuat Policy role
                options.AddPolicy("AdminRolePolicy",
                        policy => policy.RequireRole("Admin"));
            });

            // me-register custom authorization handler
            // handler pertama dari ManageAdminRolesAndClaimsRequirement
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            // handler kedua dari ManageAdminRolesAndClaimsRequirement
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            // register DataProtectionPurposeStrings
            services.AddSingleton<DataProtectionPurposeStrings>();


            // Mengubah default name dari Access denied route
            services.ConfigureApplicationCookie(config =>
            {
                config.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

            // Register External Provider 
            services.AddAuthentication()
                    // Google Oauth
                    // Client ID = 1029029722190-jlrqb0l18f28874m3lrog7ttqmaq1d72.apps.googleusercontent.com
                    // Client Secret = 5_04SH20_KFAYf_wmy8ipU_B
                    .AddGoogle(options =>
                    {
                        options.ClientId = "1029029722190-jlrqb0l18f28874m3lrog7ttqmaq1d72.apps.googleusercontent.com";
                        options.ClientSecret = "5_04SH20_KFAYf_wmy8ipU_B";
                    })
                    // Facebook Oauth
                    // AppId = 265694224715923
                    // AppSecret = e517aae2adbaee6669cd45700f16df8c
                    .AddFacebook(options =>
                    {
                        options.AppId = "265694224715923";
                        options.AppSecret = "e517aae2adbaee6669cd45700f16df8c";
                    });

            // secara default DataProtectionTokenProvider, membuat lifespan token selama 1 hari, 
            // kita bisa mengubahnya, example = ubah hanya menjadi 5 jam
            // jika kita mengubah nilai defaultnya, maka seluruh generate token life span juga akan berubah sama dengan value yang kita berikan
            // berlaku untuk GenerateEmailConfirmationToken dan GenerateResetPasswordToken
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(5));

            // kita gunakan CustomEmailConfirmationTokenProviderOptions class, dan disini kita mengubah lifespan tokennya menjadi 3 hari
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromDays(3));
        }
        #endregion

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure(), mengatur saluran pemroses permintaan aplikasi
            if (env.IsDevelopment())
            {
                // UseDeveloperExceptionPage(), menyajikan developer exception page, jika terjadi exception atau error
                // Jika terjadi exception, UseDeveloperExceptionPage akan ditampilkan di browser. dan UseDeveloperExceptionPage, harus di taruh di awal
                // Karena untuk menangkap exception yang ada di middlewares
                // Kita bisa mengcustom tampilan dari developer exception page, dengan membuat object dari DeveloperExceptionPageOptions
                // dan passing ke argumentnya. karena setiap middleware memiliki beberapa overload
                //DeveloperExceptionPageOptions devOptions = new DeveloperExceptionPageOptions()
                //{
                //    SourceCodeLineCount = 1 // menetapkan berapa banyak baris kode untuk disertakan sebelum dan sesudah baris kode yang menyebabkan pengecualian.
                //};
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // ASP.NET Core punya beberapa middleware yang berguna ketika kita mendapatkan error 404
                // 1. app.UseStatusCodePages()
                // 2. app.UseStatusCodePagesWithRedirects()
                // 3. app.UseStatusCodePagesWithReExecute()
                /* Perbedaaan antara UseStatusCodePagesWithRedirects vs UseStatusCodePagesWithReExecute
                 * UseStatusCodePagesWithRedirects()
                 * ketika kita request ke url yang tidak ada di aplikasi kita
                 * 404 status muncul, lalu UseStatusCodePagesWithRedirects middleware mencegat ini dan mengubahnya ke 302, lalu mengarahkannya ke jalur error (di sini "Error/404")
                 * status 302 artinya url dari request sumber telah di ubah sementara, disini diubah ke "/Error/404"
                 * jadi terjadi GET request ke redirect request
                 * karena redirect request, URL dari address bar juga berubah dari /foo/bar ke /Error/404
                 * request melalui middleware pipeline dan ditangani oleh MVC middleware yang akhirnya mengembalikan view NotFound dengan kode status 200 (yang berarti berhasil)
                 * Sejauh menyangkut browser dalam seluruh aliran permintaan ini, tidak ada kesalahan 404.
                 * Jika Anda mengamati dengan seksama permintaan dan aliran respons ini, kami mengembalikan kode status sukses (200) ketika sebenarnya terjadi kesalahan yang secara semantik tidak benar.
                 * 
                 * UseStatusCodePagesWithReExecute()
                 * ketika kita request ke url yang tidak ada di aplikasi kita
                 * 404 status muncul, lalu UseStatusCodePagesWithReExecute middleware mencegat ini dan mengeksekusi ulang pipeline dan merujuk ke URL (/Error/404)
                 * request melalui middleware pipeline dan ditangani oleh MVC middleware yang akhirnya mengembalikan view NotFound dengan kode status 200 (yang berarti berhasil)
                 * Saat respons menuju ke klien, ia melewati middleware UseStatusCodePagesWithReExecute yang menggunakan respons HTML tetapi mengganti 200 kode status dengan kode status 404 asli.
                 * karena ini hanya mengeksekusi ulang pipeline dan tidak melakukan redirect, maka dari itu URL asli tidak di ubah dari /foo/bar ke /Error/404
                 */
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            // UseDefaultFiles(), middleware dimana kita memberikan default file static file, yang harus bernama default.html
            // UseDefaultFiles(), memiliki beberapa overload. salah satunya kita bisa menentukan default file apa yang akan di tampilkan
            // UserFileServer(), adalah gabungan dari UseDefaultFiles() dan UseStaticFile()

            app.UseStaticFiles();
            // dibawah ini adalah middleware kita untuk bisa menggunakan MVC approach,
            // agar bisa, kita harus memanggil AddMvc() dari instance IServiceCollection di ConfigureService method

            //app.UseMvcWithDefaultRoute();
            app.UseAuthentication();// <= kita taruh setelah UseMvc middleware, karena kita ingin user untuk authenticate dulu baru bisa menuju web kita
            app.UseMvc(routes =>
            {
                //Contoh menggunakan conventional route
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });


            //app.UseDefaultFiles(defaultFiles);

            //// UseStaticFiles(), middleware untuk menyajikan static file seperti html,css dan javascript
            //app.UseStaticFiles();

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync
            //    ("Hello world");
            //});
        }
    }
}
