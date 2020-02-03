using System;
using System.IO;
using System.Threading.Tasks;
using PiBo.Shared;

namespace PiBo.Core
{
    public interface IDatabaseInterface
    {
        Task AddNewFoto(ImageInfo imageInfo, string filename, Stream[] file);
    }
}
