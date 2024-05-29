using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;

namespace TTTN3.Models
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext()
        {
        }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<product_Image> product_Images { get; set; }
        public DbSet<invoice_Detail> invoice_Details { get; set; }
        public DbSet<blog> blogs { get; set; }
        public DbSet<rating> ratings { get; set; }
        public DbSet<brand> brands { get; set; }
        public DbSet<invoice> invoices { get; set; }
        public DbSet<material> materials { get; set; }
        public DbSet<size> sizes { get; set; }
        public DbSet<product_Size> product_Sizes { get; set; }
        public DbSet<product_Color> product_Colors { get; set; }
        public DbSet<wheel> wheels { get; set; }
        public DbSet<zipper> zippers { get; set; }
        public DbSet<color> colors { get; set; }
        public DbSet<product> products { get; set; }
        public DbSet<admin> admins { get; set; }
        public DbSet<contact> contacts { get; set; }
        public DbSet<comment> comments { get; set; }
        public DbSet<FavoriteProduct> FavoriteProduct { get; set; }
        public DbSet<promotion> promotion { get; set; }
        public DbSet<guarantee> guarantee { get; set; }
    }
}
