using LafiseBancaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LafiseBancaApi.Data
{
    public class BancaContext : DbContext
    {
        public BancaContext(DbContextOptions<BancaContext> options) : base(options)
        {
        }

        // Aquí registramos nuestras tablas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración adicional para asegurar decimales correctos en SQLite
            // (SQLite a veces trata los decimales como double, esto ayuda a mantener la precisión financiera)

            modelBuilder.Entity<Cuenta>()
                .Property(c => c.Saldo)
                .HasConversion<double>(); // Truco para SQLite el cual no soporta decimal nativo perfecto

            modelBuilder.Entity<Transaccion>()
                .Property(t => t.Monto)
                .HasConversion<double>();

            modelBuilder.Entity<Transaccion>()
                .Property(t => t.SaldoDespues)
                .HasConversion<double>();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Ingresos)
                .HasConversion<double>();
        }
    }
}