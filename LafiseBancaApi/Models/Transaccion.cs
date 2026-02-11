using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LafiseBancaApi.Models
{
    public class Transaccion
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public string Tipo { get; set; } = string.Empty; // "Deposito" o "Retiro"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // Requisito clave del PDF: guardar el saldo resultante
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoDespues { get; set; }

        // Foreign Key
        public int CuentaId { get; set; }
        public Cuenta? Cuenta { get; set; }
    }
}