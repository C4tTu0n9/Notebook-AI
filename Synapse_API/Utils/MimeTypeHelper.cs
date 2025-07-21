namespace Synapse_API.Utils
{
    public class MimeTypeHelper
    {
        public static string GetMimeType(string url)
        {
            var extension = Path.GetExtension(url)?.ToLowerInvariant();
            if (extension != null && extension.StartsWith("."))
            {
                extension = extension.Substring(1);
            }

            return MapMimeType(extension);
        }

        private static string MapMimeType(string extension)
        {
            switch (extension)
            {
                case "pdf":
                    return "application/pdf";
                case "doc":
                    return "application/msword";
                case "txt":
                    return "text/plain";
                case "csv":
                    return "text/csv";
                case "xls":
                    return "application/vnd.ms-excel";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "ppt":
                    return "application/vnd.ms-powerpoint";
                case "pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.sheet";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "jpg":
                    return "image/jpeg";
                case "jpeg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                case "bmp":
                    return "image/bmp";
                case "tiff":
                    return "image/tiff";
                case "ico":
                    return "image/x-icon";
                case "webp":
                    return "image/webp";
                case "svg":
                    return "image/svg+xml";
                case "mp3":
                    return "audio/mpeg";
                case "mp4":
                    return "video/mp4";
                case "avi":
                    return "video/x-msvideo";
                case "mov":
                    return "video/quicktime";
                case "wmv":
                    return "video/x-ms-wmv";
                case "mpg":
                    return "video/mpeg";
                case "mpeg":
                    return "video/mpeg";
                case "m4v":
                    return "video/mp4";
                case "m4a":
                    return "audio/mp4";
                case "m4b":
                    return "audio/mp4";
                default:
                    return "application/octet-stream"; // fallback MIME type
            }
        }
    }
}
