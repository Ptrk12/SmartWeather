
using Microsoft.AspNetCore.Http;
using Models.requests;
using Models.SqlEntities;

namespace Interfaces.Managers
{
    public interface IImageManager
    {
        bool CheckIfImage(IFormFile? file);
        Task<string> UploadImage(Device? device, CreateDeviceReq req);
        void DeleteImageAsync(string deviceSerialNumber);
    }
}
