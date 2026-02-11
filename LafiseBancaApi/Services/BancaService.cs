using LafiseBancaApi.Data;
using LafiseBancaApi.DTOs;
using LafiseBancaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LafiseBancaApi.Services
{
    public class BancaService : IBancaService
    {
        private readonly BancaContext _context;

        public BancaService(BancaContext context)
        {
            _context = context;
        }

        public async Task<Cliente> CrearClienteAsync(CrearClienteDto dto)
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                FechaNacimiento = dto.FechaNacimiento,
                Sexo = dto.Sexo,
                Ingresos = dto.Ingresos
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<Cuenta> CrearCuentaAsync(CrearCuentaDto dto)
        {
            // validar si el cliente existe
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null) throw new Exception("Cliente no encontrado.");

            // validar unicidad de cuenta
            bool existe = await _context.Cuentas.AnyAsync(c => c.NumeroCuenta == dto.NumeroCuenta);
            if (existe) throw new Exception("El número de cuenta ya existe.");

            var cuenta = new Cuenta
            {
                ClienteId = dto.ClienteId,
                NumeroCuenta = dto.NumeroCuenta,
                Saldo = dto.SaldoInicial
            };

            // si hay saldo inicial, registramos el primer movimiento automaticamente
            if (dto.SaldoInicial > 0)
            {
                var transaccion = new Transaccion
                {
                    Fecha = DateTime.Now,
                    Tipo = "Apertura",
                    Monto = dto.SaldoInicial,
                    SaldoDespues = dto.SaldoInicial,
                    Cuenta = cuenta // EF Core maneja la relacion automaticamente
                };
                _context.Transacciones.Add(transaccion);
            }

            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();
            return cuenta;
        }

        public async Task<decimal> ConsultarSaldoAsync(string numeroCuenta)
        {
            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
            if (cuenta == null) throw new Exception("Cuenta no encontrada.");
            return cuenta.Saldo;
        }

        public async Task<Transaccion> RealizarDepositoAsync(DepositoRetiroDto dto)
        {
            if (dto.Monto <= 0) throw new Exception("El monto debe ser positivo.");

            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == dto.NumeroCuenta);
            if (cuenta == null) throw new Exception("Cuenta no encontrada.");

            cuenta.Saldo += dto.Monto;

            var transaccion = new Transaccion
            {
                CuentaId = cuenta.Id,
                Fecha = DateTime.Now,
                Tipo = "Deposito",
                Monto = dto.Monto,
                SaldoDespues = cuenta.Saldo
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return transaccion;
        }

        public async Task<Transaccion> RealizarRetiroAsync(DepositoRetiroDto dto)
        {
            if (dto.Monto <= 0) throw new Exception("El monto debe ser positivo.");

            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == dto.NumeroCuenta);
            if (cuenta == null) throw new Exception("Cuenta no encontrada.");

            // validacion de fondos 
            if (cuenta.Saldo < dto.Monto) throw new Exception("Fondos insuficientes.");

            cuenta.Saldo -= dto.Monto;

            var transaccion = new Transaccion
            {
                CuentaId = cuenta.Id,
                Fecha = DateTime.Now,
                Tipo = "Retiro",
                Monto = dto.Monto,
                SaldoDespues = cuenta.Saldo
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return transaccion;
        }

        public async Task AplicarInteresesAsync()
        {
            // aqui simulamos aplicar 5% de interes a todas las cuentas simulando un cierre de mes
            var cuentas = await _context.Cuentas.ToListAsync();
            foreach (var cuenta in cuentas)
            {
                decimal interes = cuenta.Saldo * 0.05m;
                cuenta.Saldo += interes;

                var transaccion = new Transaccion
                {
                    CuentaId = cuenta.Id,
                    Fecha = DateTime.Now,
                    Tipo = "Interes Mensual",
                    Monto = interes,
                    SaldoDespues = cuenta.Saldo
                };
                _context.Transacciones.Add(transaccion);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaccion>> ObtenerHistorialAsync(string numeroCuenta)
        {
            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
            if (cuenta == null) throw new Exception("Cuenta no encontrada.");

            // traemos las transacciones ordenadas por fecha (mas reciente primero)
            return await _context.Transacciones
                                 .Where(t => t.CuentaId == cuenta.Id)
                                 .OrderByDescending(t => t.Fecha)
                                 .ToListAsync();
        }
    }
}