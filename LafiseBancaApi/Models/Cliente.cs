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

        public char Sexo { get; set; } // 'M' o 'F' preferiblemente

        public decimal Ingresos { get; set; } // siempre usa decimal para dinero

        // relacion: Un cliente tiene muchas cuentas
        public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    }
}
