using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LafiseBancaApi.Models
{
    public class Cuenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NumeroCuenta { get; set; } = string.Empty; // Debe ser único

        [Column(TypeName = "decimal(18,2)")]
        public decimal Saldo { get; set; }

        // Foreign Key
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Relación: Una cuenta tiene muchas transacciones
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}