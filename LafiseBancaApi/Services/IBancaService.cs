using LafiseBancaApi.DTOs;
using LafiseBancaApi.Models;

namespace LafiseBancaApi.Services
{
    public interface IBancaService
    {
        Task<Cliente> CrearClienteAsync(CrearClienteDto dto);
        Task<Cuenta> CrearCuentaAsync(CrearCuentaDto dto);
        Task<decimal> ConsultarSaldoAsync(string numeroCuenta);
        Task<Transaccion> RealizarDepositoAsync(DepositoRetiroDto dto);
        Task<Transaccion> RealizarRetiroAsync(DepositoRetiroDto dto);
        Task AplicarInteresesAsync();
        Task<IEnumerable<Transaccion>> ObtenerHistorialAsync(string numeroCuenta);
    }
}