using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Learn_ASP.NET_CORE.Models;
using Learn_ASP.NET_CORE.Security;
using Learn_ASP.NET_CORE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog.Fluent;

namespace Learn_ASP.NET_CORE.Controllers
{
    // Nama Home akan menjadi nama view yang berisikan semua view dari HomeController
    // Route attribute biasanya digunakan untuk membuat route di REST API
    /* [Route("Home")]*/ // <= contoh kita menggunakan route attribute kepada controller, dengan begini kita hanya tinggal memanggi spesifik route saja di bagian action methodnya, dan tidak perlu menulis nama controllernya lagi
    /* [Route("[controller]")]*/ // <= contoh kita menggunakan token replacement. gunakan kurung siku, jika di controller namanya "controller". dengan menggunakan token replacement nama route nya akan menyesuaikan dari nama controllernya
    //[Route("[controller]/[action]")] // <= contoh kita menggabungkan controller dan action token, jadi nantinya kita hanya tinggal memberikan parameter saja, karena routenya akan di sesuaikan dengan nama controller dan nama action methodnya
    [Authorize] // <= Authorize attribute, memungkinkan hanya otentikasi user saja yang bisa mengaksesnya
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger logger;
        private readonly IDataProtector protector;

        /* HomeController bergantung pada IEmployeeRepository untuk mengambil data emplyee.
         * alih-alih kita membuat instnace dari IEmployeeRepository. kita melakukan injecting IEmployeeRepository
         * instance ke HomeController constructor. Ini disebut injeksi konstruktor, karena kami menggunakan konstruktor untuk menyuntikkan dependensi.
         */
        public HomeController(
            IEmployeeRepository employeeRepository,
            IHostingEnvironment hostingEnvironment,
            ILogger<HomeController> logger,
            IDataProtectionProvider dataProtectionProvider,
            DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {

            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            protector = dataProtectionProvider.
                CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

        //public HomeController(IEmployeeRepository employeeRepository)
        //{
        //    _employeeRepository = employeeRepository;
        //}

        // Method di controller biasanya memiliki access modifier public,
        // Method-method ini disebut action method
        //[Route("~/Home")] // <= contoh menggunakan route attribute di action method, jika kita memberikan string kosong, berarti kita jadikan root route
        //[Route("[action]")]// <= contoh menggunakan token replacement di action method, maka namanya "action". dengan kita menggunakan token replacement, nama dari route akan menyesuaikan nama dari action methodnya
        //[Route("~/")]// <= jika kita menggunakan "/" atau "~/", controller tidak akan di gabung dengan invidual action method
        [AllowAnonymous] // <= AllownAnonymous attribute, membuat anonymous user bisa mengakses route tersebut
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployee()
                        .Select(e =>
                        {
                            // kita gunakan method Proect() untuk melakukan enksripsi
                            // kita menyimpan enksripsi employee id ke properti enksripsiId
                            e.EnkripsiId = protector.Protect(e.Id.ToString());
                            return e;
                        });
            return View(model);
        }

        // MVC, akan mencari view dengan nama yang sama dengan controllernya
        // Karena disini action methodnya bernama Details maka view akan mencari file cshtml dengan nama Details
        //[Route("{id?}")] // <= contoh kita menggunakan route parameter, secara default route parameter itu wajib, kita bisa menjadikannya optional dengan menambahkan "?" di akhiran
        [AllowAnonymous]
        public ViewResult Details(string id)// <= karena kita jadikan id route parameter menjadi optional, kita juga harus menjadikan parameter dari method default menjadi nullable
        {
            //throw new Exception("details exception");
            logger.LogTrace("Trace log");
            logger.LogDebug("Debug log");
            logger.LogInformation("Information log");
            logger.LogWarning("Warning log");
            logger.LogError("Error log");
            logger.LogCritical("Critical log");
            // View method memiliki overload, yang memungkingkan kita untuk memberikan view yang tidak harus sama dengan nama Action methodnya

            // kita mengdekripsi parameter id
            int decryptedIntId = Convert.ToInt32(protector.Unprotect(id));

            /* View() atau View(object model): mencari view file dengan nama yang sama dengan action methodnya
             * View(string name): mencari view file dengan custom name
             * kalau kita menggunakan absolute path kita harus menambahkan ".cshtml" di akhir file views
               jika relative path kita tidak perlu menambahkan ".cshtml" di akhir file view
               View(string name, object model)
             */
            /* Ada 3 cara untuk passing data to view
             * 1. ViewData, menggunakan string keys untuk menyimpan data dan menerima data
             * 2. ViewBag, meggunakan dynamic properties untuk menimpan data dan menerima data
             * 3. Strongly typed view, ini yang paling bagus dari ketiganya. mengapa? karena kita bisa 
             *    mendapatkan intelisense dan compile-time type checking
             * 
             * di bawah ini kita contoh menggunakan ViewData
             */
            Employee employee = _employeeRepository.GetEmployee(decryptedIntId);
            if (employee == null)
            {
                // jika employee yang dicari tidak ada, maka kita berikan status error 404, dan alihkan user ke view "EmployeeNotFound"
                Response.StatusCode = 404;
                return View("EmployeeNotFound", decryptedIntId);
            }
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel
            {
                Employee = employee, // <= kita cek, jika id parameter memiliki value, maka kita gunakan id value. jika tidak maka 1 akan menjadi defaultnya
                PageTitle = "Employee Details"
            };
            // untuk menggunakan stornly typed view, kita bisa passing data ke argument di view method,
            // view method memiliki overload yang nantinya bisa kita render di view
            return View(homeDetailsViewModel);
        }


        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEdit = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEdit);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Menerima employee berdasarkan id dari model yang kita hidden 
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                // update employee properti yang sudah kita dapatkan
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                // jika user ingin mengupload photo,
                // maka kita cek apakah properti Photo dari model null atau tidak
                // jika null maka gunakan foto default
                if (model.Photo != null)
                {
                    // jika user mengupload maka gunakan photo yang di upload user
                    // lalu kita hapus photo yang ada di model untuk di gantikan dengan photo yang di upload user
                    // jadi di sini kita cek apakah ada photo yang sudah ada
                    // jika ada maka kita hapus
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    // simpan photo baru di wwwroot/images folder dan update PhotoPath properti
                    // dari objek employee yang akan di save di databases
                    employee.PhotoPath = ProcessUploadedFile(model); // ProcessUploadedFile(), adalah method untuk kita menerima nama file unik agar tidak terjadi duplikat
                }
                // Panggil Update(), method dari _employeeRepository dan update objek employee ke tabel databases
                Employee updatedEmployee = _employeeRepository.Update(employee);

                return RedirectToAction("index");
            }

            return View(model);
        }


        [HttpGet] // <= disini kita memberi tahu bahwa ini create method untuk GET
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]  // <= disini kita memberi tahu bahwa ini create method untuk POST
        public IActionResult Create(EmployeeCreateViewModel model) // <= ubah menjadi EmployeeCreateViewModel, untuk menyimpan gambar. karena properti Photo bertipe IFormFile
        {
            if (ModelState.IsValid) // <= cek jika validasi berhasil, jika berhasil maka kita akan menambahkan employee, jika gagal. kita akan me-render view create lagi dengan error message agar user tahu salahnya dimana
            {
                // kita membuat variabel untuk menyimpan nama file unik
                string uniqueFileName = ProcessUploadedFile(model);

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };
                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });// <= ketika kita sudah menambah employee, kita langsung redirect ke details dari employee tersebut
            }
            return View(); // <= jika gagal, maka user akan di kembalikan ke view create dengan pesan error
        }

        // Function untuk mengupload gambar
        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
    }
}
