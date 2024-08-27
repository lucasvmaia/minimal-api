using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Context
{
    public class MyContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public MyContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador {
                    Id = 1,
                    Email = "administrador@teste.com",
                    Senha = "12345",
                    Perfil = "adm"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var stringConn = _configuration.GetConnectionString("conn")?.ToString();
                if(!string.IsNullOrEmpty(stringConn))
                {
                    //optionsBuilder.UseSqlServer("Server=localhost;Database=MyDatabase;User Id=myuser;Password=mypassword;");
                    optionsBuilder.UseSqlServer(stringConn);
                }
            }
        }
    }
}