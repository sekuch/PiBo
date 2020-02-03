using Microsoft.Extensions.Hosting;
using PiBo.Services.FolderImport;
using System.Threading;
using System.Threading.Tasks;

namespace PiBo.Server
{
    internal class FolderImportHostedService : IHostedService
    {
        private FolderImportService _service;

        public FolderImportHostedService(FolderImportService service)
        {
            _service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _service.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}