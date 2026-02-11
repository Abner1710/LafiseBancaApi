namespace LafiseBancaApi.DTOs
{
    public class CrearClienteDto
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public char Sexo { get; set; }
        public decimal Ingresos { get; set; }
    }
}