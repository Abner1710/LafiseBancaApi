using LafiseBancaApi.Data;
using LafiseBancaApi.DTOs;
using LafiseBancaApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit; // libreria de pruebas

namespace LafiseBancaApi.Tests
{
    public class BancaServiceTests
    {
        // metodo auxiliar para crear una base de datos fantasma o momentanea unica para cada prueba
        private BancaContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<BancaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // nombre unico para no mezclar datos entre pruebas
                .Options;

            return new BancaContext(options);
        }

        [Fact] // etiqueta que indica que esto es una prueba unitaria
        public async Task CrearCuenta_DebeGuardarEnBaseDeDatos()
        {
            // 1. Preparar el escenario (Arrange)
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // necesitamos un cliente previo
            var cliente = new Models.Cliente { Nombre = "Tester", Ingresos = 1000 };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var dto = new CrearCuentaDto
            {
                ClienteId = cliente.Id,
                NumeroCuenta = "TEST-001",
                SaldoInicial = 500
            };

            // 2. Ejecutar la accion (Act)
            var cuentaCreada = await service.CrearCuentaAsync(dto);

            // 3. Verificar el resultado (Assert)
            Assert.NotNull(cuentaCreada); // se creo el objeto?
            Assert.Equal(500, cuentaCreada.Saldo); // el saldo es correcto?
            Assert.Equal("TEST-001", cuentaCreada.NumeroCuenta); // el numero coincide?

            // verificamos que se haya creado la transaccion de apertura
            var transaccion = await context.Transacciones.FirstOrDefaultAsync(t => t.CuentaId == cuentaCreada.Id);
            Assert.NotNull(transaccion);
            Assert.Equal("Apertura", transaccion.Tipo);
        }


        [Fact]
        public async Task RealizarDeposito_DebeAumentarSaldo()
        {
            // 1. Arrange
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // creamos una cuenta con saldo 0
            var cuenta = new Models.Cuenta { NumeroCuenta = "DEP-001", Saldo = 0, ClienteId = 1 };
            context.Cuentas.Add(cuenta);
            await context.SaveChangesAsync();

            var dto = new DepositoRetiroDto { NumeroCuenta = "DEP-001", Monto = 100 };

            // 2. Act
            var transaccion = await service.RealizarDepositoAsync(dto);

            // 3. Assert
            Assert.Equal(100, transaccion.SaldoDespues); // el saldo debe ser 100

            var cuentaDb = await context.Cuentas.FirstAsync(c => c.NumeroCuenta == "DEP-001");
            Assert.Equal(100, cuentaDb.Saldo); // verificamos que se guardo en la BD falsa
        }

        [Fact]
        public async Task RealizarRetiro_ConFondosSuficientes_DebeDisminuirSaldo()
        {
            // 1. Arrange
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // cuenta con 500 de saldo
            var cuenta = new Models.Cuenta { NumeroCuenta = "RET-001", Saldo = 500, ClienteId = 1 };
            context.Cuentas.Add(cuenta);
            await context.SaveChangesAsync();

            var dto = new DepositoRetiroDto { NumeroCuenta = "RET-001", Monto = 200 };

            // 2. Act
            var transaccion = await service.RealizarRetiroAsync(dto);

            // 3. Assert
            Assert.Equal(300, transaccion.SaldoDespues); // 500 - 200 = 300
        }

        [Fact]
        public async Task RealizarRetiro_SinFondosSuficientes_DebeLanzarExcepcion()
        {
            // 1. Arrange
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // cuenta con solo 100 pesitos
            var cuenta = new Models.Cuenta { NumeroCuenta = "POBRE-001", Saldo = 100, ClienteId = 1 };
            context.Cuentas.Add(cuenta);
            await context.SaveChangesAsync();

            var dto = new DepositoRetiroDto { NumeroCuenta = "POBRE-001", Monto = 5000 };

            // 2. Act & Assert
            // aqui esperamos que el codigo "explote" controladamente y capture la excepcion
            var excepcion = await Assert.ThrowsAsync<Exception>(() =>
                service.RealizarRetiroAsync(dto));

            Assert.Equal("Fondos insuficientes.", excepcion.Message);
        }

        [Fact]
        public async Task AplicarIntereses_DebeAumentarSaldoDeTodasLasCuentas()
        {
            // 1. Arrange
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // dos cuentas: una con 100 y otra con 200
            context.Cuentas.AddRange(
                new Models.Cuenta { NumeroCuenta = "INT-1", Saldo = 100, ClienteId = 1 },
                new Models.Cuenta { NumeroCuenta = "INT-2", Saldo = 200, ClienteId = 1 }
            );
            await context.SaveChangesAsync();

            // 2. Act (aqui hay que aplicamos 5% de interes)
            await service.AplicarInteresesAsync();

            // 3. Assert
            var cuenta1 = await context.Cuentas.FirstAsync(c => c.NumeroCuenta == "INT-1");
            var cuenta2 = await context.Cuentas.FirstAsync(c => c.NumeroCuenta == "INT-2");

            Assert.Equal(105, cuenta1.Saldo); // 100 + 5% = 105
            Assert.Equal(210, cuenta2.Saldo); // 200 + 5% = 210
        }

        [Fact]
        public async Task ConsultarSaldo_DebeRetornarMontoCorrecto()
        {
            // 1. Arrange
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // Creamos una cuenta con 1500 exactos
            context.Cuentas.Add(new Models.Cuenta { NumeroCuenta = "SALDO-001", Saldo = 1500, ClienteId = 1 });
            await context.SaveChangesAsync();

            // 2. Act
            var saldo = await service.ConsultarSaldoAsync("SALDO-001");

            // 3. Assert
            Assert.Equal(1500, saldo);
        }

        [Fact]
        public async Task ObtenerHistorial_DebeRetornarListaOrdenada()
        {
            // 1. Arrange
            var context = GetInMemoryDbContext();
            var service = new BancaService(context);

            // Creamos cuenta
            var cuenta = new Models.Cuenta { NumeroCuenta = "HIST-001", Saldo = 1000, ClienteId = 1 };
            context.Cuentas.Add(cuenta);
            await context.SaveChangesAsync();

            // Agregamos 2 transacciones manualmente a la BD con fechas distintas
            context.Transacciones.AddRange(
                new Models.Transaccion { CuentaId = cuenta.Id, Tipo = "Depósito", Monto = 500, Fecha = DateTime.Now.AddDays(-2), SaldoDespues = 500 }, // Vieja
                new Models.Transaccion { CuentaId = cuenta.Id, Tipo = "Retiro", Monto = 100, Fecha = DateTime.Now, SaldoDespues = 400 } // Reciente
            );
            await context.SaveChangesAsync();

            // 2. Act
            var historial = await service.ObtenerHistorialAsync("HIST-001");

            // 3. Assert
            Assert.Equal(2, historial.Count()); // Deben ser 2
            Assert.Equal("Retiro", historial.First().Tipo); // La primera debe ser la más reciente (Orden Descendente)
        }
    }
}