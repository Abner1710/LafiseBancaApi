using LafiseBancaApi.DTOs;
using LafiseBancaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LafiseBancaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BancaController : ControllerBase
    {
        private readonly IBancaService _bancaService;

        // Inyeccion de Dependencias del Servicio
        public BancaController(IBancaService bancaService)
        {
            _bancaService = bancaService;
        }

        // 1. Crear Cliente
        [HttpPost("clientes")]
        public async Task<IActionResult> CrearCliente([FromBody] CrearClienteDto dto)
        {
            var cliente = await _bancaService.CrearClienteAsync(dto);
            return Ok(cliente);
        }

        // 2. Crear Cuenta
        [HttpPost("cuentas")]
        public async Task<IActionResult> CrearCuenta([FromBody] CrearCuentaDto dto)
        {
            try
            {
                var cuenta = await _bancaService.CrearCuentaAsync(dto);
                return Ok(new { Mensaje = "Cuenta creada exitosamente", Cuenta = cuenta });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 3. Consultar Saldo
        [HttpGet("cuentas/{numeroCuenta}/saldo")]
        public async Task<IActionResult> ConsultarSaldo(string numeroCuenta)
        {
            try
            {
                var saldo = await _bancaService.ConsultarSaldoAsync(numeroCuenta);
                return Ok(new { NumeroCuenta = numeroCuenta, SaldoActual = saldo });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // 4. Registrar Deposito
        [HttpPost("transacciones/deposito")]
        public async Task<IActionResult> RealizarDeposito([FromBody] DepositoRetiroDto dto)
        {
            try
            {
                var transaccion = await _bancaService.RealizarDepositoAsync(dto);
                return Ok(new { Mensaje = "Depósito realizado", NuevoSaldo = transaccion.SaldoDespues, TransaccionId = transaccion.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 5. Registrar Retiro
        [HttpPost("transacciones/retiro")]
        public async Task<IActionResult> RealizarRetiro([FromBody] DepositoRetiroDto dto)
        {
            try
            {
                var transaccion = await _bancaService.RealizarRetiroAsync(dto);
                return Ok(new { Mensaje = "Retiro realizado", NuevoSaldo = transaccion.SaldoDespues, TransaccionId = transaccion.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // aqui saldra el error de fondos insuficientes
            }
        }

        // 6. Historial de Transacciones
        [HttpGet("cuentas/{numeroCuenta}/historial")]
        public async Task<IActionResult> ObtenerHistorial(string numeroCuenta)
        {
            try
            {
                var historial = await _bancaService.ObtenerHistorialAsync(numeroCuenta);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // 7. Simular Cierre de Mes (Aplicar Intereses)
        [HttpPost("cierre-mensual")]
        public async Task<IActionResult> AplicarIntereses()
        {
            try
            {
                await _bancaService.AplicarInteresesAsync();
                return Ok(new { Mensaje = "Cierre mensual ejecutado. Se han aplicado intereses del 5% a todas las cuentas." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}