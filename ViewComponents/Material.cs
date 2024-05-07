using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using TTTN3.Models;

namespace TTTN3.ViewComponents
{
    public class Material : ViewComponent
    {
        private readonly DataContext db;
        public Material(DataContext dba)
        {
            db = dba;

        }
        public IViewComponentResult Invoke(string material_Code)
        {
            if(material_Code != null)
            {
                ViewBag.Material_Code = material_Code;
            }
            // var blogs = db.GetAllBlog(3);
            var materials = db.materials.ToList();
            return View("Material_List", materials);
        }
    }
}
