using PiBo.Core;
using PiBo.Shared;
using Raven.Client.Documents;
using Raven.Embedded;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PiBo.DB.EmbeddedRavenDb
{
    public class EmbeddedRavenDbInterface : IDatabaseInterface
    {
        private static IDocumentStore _store;

        static EmbeddedRavenDbInterface()
        {
            EmbeddedServer.Instance.StartServer();
            EmbeddedServer.Instance.OpenStudioInBrowser();
        }

        private static IDocumentStore Store
        {
            get
            {
                if (_store == null)
                {
                    _store = EmbeddedServer.Instance.GetDocumentStore("PiBo");
                }
                return _store;
            }
        }

        public async Task AddNewFoto(ImageInfo imageInfo, string filename, Stream file)
        {
            using (var session = _store.OpenAsyncSession())
            {
                await session.StoreAsync(imageInfo).ConfigureAwait(false);
                session.Advanced.Attachments.Store(imageInfo, filename, file);   
            }
        }
    }
}
