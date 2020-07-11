using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Learn_ASP.NET_CORE
{
    public class Program
    {
        /* Main method adalah entry point kita untuk membuat asp.net core application
         * ASP.NET CORE web app, di inisialisasi sebagai console application, maka dari itu kenapa ada
         * Main method
         * Jadi, ketika runtime mengeksekusi aplikasi kita, ia mencari metode Main () dan ini tempat eksekusi dimulai.
         */
        public static void Main(string[] args)
        {
            // CreateWebHostBuilder () mengembalikan objek yang mengimplementasikan IWebHostBuilder.
            // Pada objek ini, metode Build () dipanggil yang membangun host web yang menampung aplikasi web asp.net core
            // Di web host, metode Run () dipanggil, yang menjalankan aplikasi web dan mulai mendengarkan permintaan HTTP yang masuk.
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                    // Enable NLog as one of the Logging Provider
                    logging.AddNLog();
                })
                // CreateDefaultBuilder(), melakukan beberapa tugas diantaranya;
                // 1. Menyiapkan web server
                // 2. Memuat konfigurasi host dan aplikasi dari berbagai sumber konfigurasi
                // 3. Mengkonfigurasi logging
                // Disini ASP.NET Core bisa di hosting dengan 2 cara pertama InProcess dan kedua OutOfProcess

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
