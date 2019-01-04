namespace Nop.Services.Media
{
    public interface IPictureService
    {
        string GetPictureUrl(string fileName, int targetSize = 0, string storeLocation = null);
        string GetDefaultPictureUrl(int targetSize = 0, string storeLocation = null);
        string SavePicture(byte[] pictureBinary, string mimeType, int targetSize = 0);

        void DeleteUploadPicture(string fileName);
    }
}