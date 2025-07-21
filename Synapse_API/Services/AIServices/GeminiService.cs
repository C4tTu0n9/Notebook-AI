using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Web;

namespace Synapse_API.Services.AIServices
{
    public class GeminiService
    {
        private readonly IGenerativeModelService _svc;
        private readonly GenerativeModel _model;
        private readonly GoogleAI _googleAI;

        public GeminiService(IGenerativeModelService svc, GoogleAI googleAI)
        {
            _svc = svc;
            _model = _svc.CreateInstance();
            _googleAI = googleAI;
        }

        public async Task<string> GenerateContent(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return "";
            var result = await _model.GenerateContent(prompt);
            return result?.Text ?? "";
        }

        public async Task<GenerateContentResponse?> GenerateContentWithConfig(GenerateContentRequest request, GenerationConfig generationConfig)
        {
            request.GenerationConfig = generationConfig;
            var result = await _model.GenerateContent(request);
            return result;
        }

        /// <summary>
        /// Tải tệp từ URL S3, sau đó tải lên Google File API và trả về Google File URI.
        /// </summary>
        /// <param name="s3Url">URL của tệp trên AWS S3.</param>
        /// <param name="mimeType">Loại MIME của tệp (ví dụ: "application/pdf").</param>
        /// <returns>URI của tệp đã được tải lên Google File API.</returns>
        /// <exception cref="ArgumentNullException">Ném ra nếu URL S3 hoặc MIME type trống.</exception>
        /// <exception cref="HttpRequestException">Ném ra nếu quá trình tải xuống từ S3 hoặc tải lên Google File API thất bại.</exception>
        public async Task<string> UploadS3FileToGoogleFileApi(string s3Url, string mimeType)
        {
            if (string.IsNullOrEmpty(s3Url))
            {
                throw new ArgumentNullException(nameof(s3Url), "URL S3 không được để trống.");
            }
            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentNullException(nameof(mimeType), "MIME type không được để trống.");
            }

            //Tải tệp từ S3 về ứng dụng
            byte[] fileBytes;
            string fileName = Path.GetFileName(new Uri(s3Url).LocalPath);

            using (HttpClient client = new HttpClient())
            {
                fileBytes = await client.GetByteArrayAsync(s3Url);
            }

            // Tạo một MemoryStream từ byte array để tải lên
            using var memoryStream = new MemoryStream(fileBytes);
            // Sử dụng GoogleAI.UploadFile để upload stream
            var uploadResult = await _googleAI.UploadFile(
                stream: memoryStream,
                displayName: Path.GetFileNameWithoutExtension(fileName),
                mimeType: mimeType
            );

            return uploadResult.File.Uri; // Đây là URI cần dùng cho FileData
        }
    }
}
