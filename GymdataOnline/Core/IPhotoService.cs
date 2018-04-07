using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Core.PhotoServie
{
   public interface IPhotoService
    {
        Task<bool> TryUploadAsync(IFormFile formFile,string DirectoryName,string FullPath);
        bool TryDelete(string DirectoryName, string FullPath);
    }
}
