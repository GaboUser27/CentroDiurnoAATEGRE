using CentroDiurnoAATEGRE.Application.Services.Interfaces;

namespace CentroDiurnoAATEGRE.Web.BackgroundServices
{
    /// <summary>
    /// Tarea programada que inactiva los avisos cuya fecha de expiración ya pasó.
    ///
    /// Corre una vez al arrancar la aplicación y luego cada <see cref="Intervalo"/>.
    /// El barrido al arranque es importante: si el sitio estuvo apagado (o IIS
    /// recicló el proceso) durante varios días, al volver a levantar se pone al día
    /// de inmediato en vez de esperar al siguiente ciclo.
    /// </summary>
    public class AvisosVencidosService : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AvisosVencidosService> _logger;

        public AvisosVencidosService(
            IServiceScopeFactory scopeFactory,
            ILogger<AvisosVencidosService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Barrido inicial.
            await DesactivarVencidosAsync(stoppingToken);

            using var timer = new PeriodicTimer(Intervalo);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DesactivarVencidosAsync(stoppingToken);
            }
        }

        private async Task DesactivarVencidosAsync(CancellationToken ct)
        {
            try
            {
                // BackgroundService es singleton y IAvisoService/DbContext son scoped,
                // así que hay que abrir un scope propio en cada ejecución.
                using var scope = _scopeFactory.CreateScope();
                var avisoService = scope.ServiceProvider.GetRequiredService<IAvisoService>();

                var afectados = await avisoService.DesactivarVencidosAsync();

                if (afectados > 0)
                    _logger.LogInformation(
                        "Avisos vencidos inactivados automáticamente: {Cantidad}.", afectados);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Apagado normal de la aplicación, no es un error.
            }
            catch (Exception ex)
            {
                // Nunca dejar que una excepción tumbe el BackgroundService: si la base
                // de datos no responde en este ciclo, se reintenta en el siguiente.
                _logger.LogError(ex, "Error al inactivar los avisos vencidos.");
            }
        }
    }
}
