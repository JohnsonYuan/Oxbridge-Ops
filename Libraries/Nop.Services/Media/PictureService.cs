using System;
using System.Drawing;
using System.IO;
using ImageResizer;
using Nop.Core;

namespace Nop.Services.Media
{
    public class PictureService : IPictureService
    {
        private IWebHelper _webHelper;
        public PictureService(IWebHelper webHelper)
        {
            this._webHelper = webHelper;
        }

        #region Utilities

        protected virtual Size CalculateDimensions(Size originalSize, int targetSize,
            ResizeType resizeType = ResizeType.LongestSide, bool ensureSizePositive = true)
        {
            float width, height;

            switch (resizeType)
            {
                case ResizeType.LongestSide:
                    if (originalSize.Height > originalSize.Width)
                    {
                        // portrait
                        width = originalSize.Width * (targetSize / (float)originalSize.Height);
                        height = targetSize;
                    }
                    else
                    {
                        // landscape or square
                        width = targetSize;
                        height = originalSize.Height * (targetSize / (float)originalSize.Width);
                    }
                    break;
                case ResizeType.Width:
                    width = targetSize;
                    height = originalSize.Height * (targetSize / (float)originalSize.Width);
                    break;
                case ResizeType.Height:
                    width = originalSize.Width * (targetSize / (float)originalSize.Height);
                    height = targetSize;
                    break;
                default:
                    throw new Exception("Not supported ResizeType");
            }

            if (ensureSizePositive)
            {
                if (width < 1)
                    width = 1;
                if (height < 1)
                    height = 1;
            }

            //we invoke Math.Round to ensure that no white background is rendered - http://www.nopcommerce.com/boards/t/40616/image-resizing-bug.aspx
            return new Size((int)Math.Round(width), (int)Math.Round(height));
        }

        /// <summary>
        /// Get picture local path. Used when images stored on file system (not in the database)
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Local picture path</returns>
        protected virtual string GetPictureLocalPath(string fileName)
        {
            return Path.Combine(CommonHelper.MapPath("~/content/upload/"), fileName);
        }

        /// <summary>
        /// Returns the file extension from mime type.
        /// </summary>
        /// <param name="mimeType">Mime type</param>
        /// <returns>File extension</returns>
        protected virtual string GetFileExtensionFromMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;

            //also see System.Web.MimeMapping for more mime types

            string[] parts = mimeType.Split('/');
            string lastPart = parts[parts.Length - 1];
            switch (lastPart)
            {
                case "pjpeg":
                    lastPart = "jpg";
                    break;
                case "x-png":
                    lastPart = "png";
                    break;
                case "x-icon":
                    lastPart = "ico";
                    break;
            }
            return lastPart;
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected virtual void SaveThumb(string thumbFilePath, byte[] binary)
        {
            File.WriteAllBytes(thumbFilePath, binary);
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected virtual string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            storeLocation = !String.IsNullOrEmpty(storeLocation)
                                    ? storeLocation
                                    : _webHelper.GetStoreLocation();
            var url = storeLocation + "content/images/thumbs/";

            url = url + thumbFileName;
            return url;
        }

        #endregion

        /// <summary>
        /// Gets the default picture URL
        /// </summary>
        /// <param name="targetSize">The target picture size (longest side)</param>

        public virtual string GetDefaultPictureUrl(int targetSize = 0,
            string storeLocation = null)
        {
            string defaultImageFileName = "default-avatar.jpg";

            return GetPictureUrl(defaultImageFileName, targetSize, storeLocation);
        }

        public virtual string GetPictureUrl(string fileName,
            int targetSize = 0,
            string storeLocation = null)
        {
            string filePath = GetPictureLocalPath(fileName);
            if (!File.Exists(filePath))
            {
                return GetDefaultPictureUrl(targetSize, storeLocation);
            }

            if (targetSize == 0)
            {
                string url = _webHelper.GetStoreLocation()
                                 + "content/upload/" + fileName;
                return url;
            }
            else
            {
                string fileExtension = Path.GetExtension(filePath);
                string thumbFileName = string.Format("{0}_{1}{2}",
                    Path.GetFileNameWithoutExtension(filePath),
                    targetSize,
                    fileExtension);

                //var thumbFilePath = GetThumbLocalPath(thumbFileName);
                var thumbFilePath = GetPictureLocalPath(thumbFileName);

                if (!File.Exists(thumbFilePath))
                {
                    using (var b = new Bitmap(filePath))
                    {
                        using (var destStream = new MemoryStream())
                        {
                            var newSize = CalculateDimensions(b.Size, targetSize);
                            ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                            {
                                Width = newSize.Width,
                                Height = newSize.Height,
                                Scale = ScaleMode.Both,
                                Quality = 90
                            });
                            var destBinary = destStream.ToArray();
                            SaveThumb(thumbFilePath, destBinary);
                        }
                    }
                }

                var url = GetThumbUrl(thumbFileName, storeLocation);
                return url;
            }
        }

        /// <summary>
        /// Save picture
        /// </summary>
        /// <param name="pictureBinary"></param>
        /// <param name="mimeType"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public virtual string SavePicture(byte[] pictureBinary, string mimeType,
            int targetSize = 0)
        {
            string lastPart = GetFileExtensionFromMimeType(mimeType);
            string fileName = string.Format("{0}_0.{1}", Guid.NewGuid().ToString("N"), lastPart);
            if (targetSize == 0)
            {
                File.WriteAllBytes(GetPictureLocalPath(fileName), pictureBinary);

                string url = _webHelper.GetStoreLocation()
                                 + "content/upload/" + fileName;
                return url;
            }
            else
            {
                string fileExtension = Path.GetExtension(fileName);
                string thumbFileName = string.Format("{0}_{1}{2}",
                    Path.GetFileNameWithoutExtension(fileName),
                    targetSize,
                    fileExtension);

                //var thumbFilePath = GetThumbLocalPath(thumbFileName);
                var thumbFilePath = GetPictureLocalPath(thumbFileName);

                using (var sourceStream = new MemoryStream(pictureBinary))
                using (var b = new Bitmap(sourceStream))
                using (var destStream = new MemoryStream())
                {
                    var newSize = CalculateDimensions(b.Size, targetSize);
                    ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                    {
                        Width = newSize.Width,
                        Height = newSize.Height,
                        Scale = ScaleMode.Both,
                        Quality = 90
                    });
                    var destBinary = destStream.ToArray();
                    SaveThumb(thumbFilePath, destBinary);

                    return thumbFileName;
                }
            }
        }

        public virtual void DeleteUploadPicture(string fileName)
        {
            string filePath = GetPictureLocalPath(fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {

                }
            }
        }
    }
}
