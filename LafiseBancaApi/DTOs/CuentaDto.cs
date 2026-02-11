namespace LafiseBancaApi.DTOs
{
    public class CrearCuentaDto
    {
        public int ClienteId { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
    }

    public class DepositoRetiroDto
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}