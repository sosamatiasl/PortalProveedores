using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shiny.Jobs;
using PortalProveedores.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace PortalProveedores.Mobile.Jobs
{
    // IJob es la interfaz que define la tarea en segundo plano.
    public class SynchronizationJob : IJob
    {
        private readonly ISynchronizationService _syncService;
        private readonly ILogger<SynchronizationJob> _logger;

        public SynchronizationJob(ISynchronizationService syncService, ILogger<SynchronizationJob> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>
        /// Lógica que se ejecuta cuando el Job es invocado por el sistema operativo.
        /// </summary>
        public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            _logger.LogInformation("Job de Sincronización iniciado por el sistema operativo.");

            // Simplemente se ejecuta la lógica central del servicio
            await _syncService.SynchronizePendingRemitosAsync();
        }
    }
}
