using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using TTTN3.Models;
//using TTTN3.Responsitory;

namespace TTTN3.ViewComponents
{
    public class Blog : ViewComponent
    {
        //private readonly IBlogResponsitory db;
        //public Blog(IBlogResponsitory dba)
        //{
        //    db = dba;

        //}
        private readonly DataContext db;
        public Blog(DataContext dba)
        {
            db = dba;

        }
        public IViewComponentResult Invoke()
        {
            // var blogs = db.GetAllBlog(3);
            var blogs = db.blogs.OrderByDescending(x=>x.blog_Date).Take(3).ToList();
            return View("Blog_List", blogs);
        }
    }
}
