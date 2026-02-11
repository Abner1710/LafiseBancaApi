using System.ComponentModel.DataAnnotations;

namespace LafiseBancaApi.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public char Sexo { get; set; } // 'M' o 'F'

        public decimal Ingresos { get; set; } // Siempre usa decimal para dinero

        // Relación: Un cliente tiene muchas cuentas
        public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    }
}
