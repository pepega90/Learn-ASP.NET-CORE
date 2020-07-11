using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learn_ASP.NET_CORE.Models;

namespace Learn_ASP.NET_CORE.ViewModels
{
    /* Disini kita membuat class untuk view model, apa itu view model?
     * yang berisi semua data yang dibutuhkan tampilan. Kelas ini disebut ViewModel
     * Nama dari class view model ini mengikuti nama dari controllernya dan action methodnya
     * nama_controller_nama_action_method_ViewModel
     * HomeControllerDetailsViewModel
     * Secara umum, kita menggunakan ViewModels untuk mengirim data antara View dan Controller
     * maka dari itu view model biasanya disebut sebagai data transfer object (DTO)
     */
    public class HomeDetailsViewModel
    {
        public Employee Employee { get; set; }
        public string PageTitle { get; set; }
    }
}
