using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderService
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private IBusControl _bus;

        public Worker(ILogger<Worker> logger, IBusControl bus)
        {
            _logger = logger;
            _bus = bus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting the bus...");
            return _bus.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping the bus...");
            return _bus.StopAsync(cancellationToken);
        }
    }
}
