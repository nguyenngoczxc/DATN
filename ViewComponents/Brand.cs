using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using TTTN3.Models;

namespace TTTN3.ViewComponents
{
    public class Brand : ViewComponent
    {
        private readonly DataContext db;
        public Brand(DataContext dba)
        {
            db = dba;

        }
        public IViewComponentResult Invoke()
        {
            // var blogs = db.GetAllBlog(3);
            var brands = db.brands.ToList();
            return View("Brand_List", brands);
        }
    }
}
