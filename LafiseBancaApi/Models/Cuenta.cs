using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LafiseBancaApi.Models
{
    public class Cuenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NumeroCuenta { get; set; } = string.Empty; // debe ser unico

        [Column(TypeName = "decimal(18,2)")]
        public decimal Saldo { get; set; }

        // Foreign Key
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // relacion: una cuenta tiene muchas transacciones
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}