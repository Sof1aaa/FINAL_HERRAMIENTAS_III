using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HERRAMIENTAS.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer("Server=tcp:sofiaserver123.database.windows.net,1433;Initial Catalog=SistemaPedidos;User ID=sofiaadmin;Password=Sofia123;Encrypt=True;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}