using Qdrant.Client; 
using Qdrant.Client.Grpc; 
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Vml;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Web;
using Synapse_API.Services.DocumentServices.Interfaces;
using UglyToad.PdfPig;
using Google.Protobuf.Collections;
using Amazon.Runtime.Internal.Transform;


namespace Synapse_API.Services.DocumentServices.Implementations
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly IGenerativeModelService _generativeModelService;
        private readonly QdrantClient _qdrantClient; 
        private readonly string _qdrantCollectionName;
        private readonly string _geminiEmbeddingModel;


        public DocumentProcessingService(IGenerativeModelService generativeModelService, IConfiguration configuration, QdrantClient qdrantClient)
        {
            _generativeModelService = generativeModelService;
            _qdrantClient = qdrantClient; // Inject QdrantClient
            _qdrantCollectionName = configuration["Qdrant:CollectionName"] ?? "synapse-learning-knowledge-base"; // Lấy từ cấu hình Qdrant
            _geminiEmbeddingModel = configuration["Gemini:EmbeddingModel"] ?? "text-embedding-004";
        }
        public async Task<bool> ProcessAndEmbedDocumentAsync(IFormFile file, int userId, string documentName, int courseId, int topicId)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("File không hợp lệ hoặc rỗng.");
                return false;
            }
            
            if (userId <= 0 || courseId <= 0 || topicId <= 0)
            {
                Console.WriteLine("userId, courseId hoặc topicId không hợp lệ.");
                return false;
            }

            // Kiểm tra trùng lặp dựa trên userId, courseId và topicId
            var filter = new Qdrant.Client.Grpc.Filter();
            filter.Must.Add(new Qdrant.Client.Grpc.Condition
            {
                Field = new FieldCondition
                {
                    Key = "user_id",
                    Match = new Match{ Integer = userId }
                }
            });
            filter.Must.Add(new Qdrant.Client.Grpc.Condition
            {
                Field = new FieldCondition
                {
                    Key = "course_id",
                    Match = new Match { Integer = courseId }
                }
            });
            filter.Must.Add(new Qdrant.Client.Grpc.Condition
            {
                Field = new FieldCondition
                {
                    Key = "topic_id",
                    Match = new Match { Integer = topicId }
                }
            });

            var existingPoints = await _qdrantClient.ScrollAsync(
                collectionName: _qdrantCollectionName,
                filter: filter,
                limit: 1
            );

            if (existingPoints.Result.Any())
            {
                // Xóa các điểm cũ liên quan đến userId, courseId và topicId
                await _qdrantClient.DeleteAsync(_qdrantCollectionName, filter);
                Console.WriteLine($"Đã xóa các điểm cũ liên quan đến userId {userId}, courseId {courseId} và topicId {topicId}.");
            }


            string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
            string fileContent = string.Empty;

            // 1. Đọc và trích xuất nội dung từ file
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // Đặt lại vị trí stream về đầu

                try
                {
                    switch (fileExtension)
                    {
                        case ".pdf":
                            fileContent = ExtractTextFromPdf(stream);
                            break;
                        case ".docx":
                            fileContent = ExtractTextFromDocx(stream);
                            break;
                        case ".xlsx":
                            fileContent = ExtractTextFromXlsx(stream);
                            break;
                        case ".txt":
                            using (var reader = new StreamReader(stream))
                            {
                                fileContent = await reader.ReadToEndAsync();
                            }
                            break;
                        default:
                            throw new NotSupportedException($"Định dạng file '{fileExtension}' không được hỗ trợ.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file.FileName}: {ex.Message}");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                Console.WriteLine($"Không trích xuất được nội dung từ file {file.FileName}.");
                return false;
            }

            // 2. Chia nhỏ nội dung thành các đoạn (chunks)
            var chunks = ChunkText(fileContent, 1000, 200); // chunk_size=1000, chunk_overlap=200

            if (!chunks.Any())
            {
                Console.WriteLine($"Không có đoạn nào được tạo từ file {file.FileName}.");
                return false;
            }


            var embeddingsList = new List<List<float>>();
            var documentsList = new List<string>();
            var metadatasList = new List<Dictionary<string, object>>();
            var idsList = new List<string>();

            // Lấy một instance của embedding model
            var embeddingModel = _generativeModelService.CreateInstance(_geminiEmbeddingModel);

            foreach (var chunk in chunks)
            {
                try
                {
                      // Tạo embedding bằng Gemini
                    var embedContentResponse = await embeddingModel.EmbedContent(new EmbedContentRequest(chunk));
                   // var embedContentResponse = await embeddingModel.EmbedContent(embedContentRequest);

                    if (embedContentResponse?.Embedding?.Values?.Any() == true)
                    {
                        embeddingsList.Add(embedContentResponse.Embedding.Values.ToList());
                        documentsList.Add(chunk);
                        metadatasList.Add(new Dictionary<string, object>
                        {
                            { "user_id", userId },
                            { "document_name", documentName },
                            { "original_filename", file.FileName },
                            { "chunk_index", chunks.IndexOf(chunk) },
                            { "text_content", chunk },
                            { "course_id", courseId},
                            { "topic_id", topicId}
                        });
                        idsList.Add(Guid.NewGuid().ToString());
                    }
                    else
                    {
                        Console.WriteLine($"Không tạo được embedding cho một đoạn từ file {file.FileName}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi tạo embedding cho một đoạn từ file {file.FileName}: {ex.Message}");
                }
            }
            if (embeddingsList.Any())
            {
                // Kiểm tra và tạo Collection nếu chưa tồn tại
                try
                {
                    await _qdrantClient.CreateCollectionAsync(
                        _qdrantCollectionName,
                        new VectorParams { Size = (ulong)embeddingsList[0].Count, Distance = Distance.Cosine } // Kích thước vector và độ đo (Cosine là phổ biến)
                    );
                    Console.WriteLine($"Đã tạo mới Qdrant collection: {_qdrantCollectionName}");
                }
                catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
                {
                    Console.WriteLine($"Qdrant collection '{_qdrantCollectionName}' đã tồn tại.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra/tạo Qdrant collection: {ex.Message}");
                    return false;
                }

                var points = new List<PointStruct>();
                for (int i = 0; i < embeddingsList.Count; i++)
                {
                    var payload = new Dictionary<string, Value>(); // Payload là Dictionary<string, Value> trong Qdrant
                    foreach (var meta in metadatasList[i])
                    {
                        if (meta.Value is string sVal)
                            payload.Add(meta.Key, new Value { StringValue = sVal });
                        else if (meta.Value is int iVal)
                            payload.Add(meta.Key, new Value { IntegerValue = iVal });
                        else if (meta.Value is bool bVal)
                            payload.Add(meta.Key, new Value { BoolValue = bVal });
                        else if (meta.Value is double dVal)
                            payload.Add(meta.Key, new Value { DoubleValue = dVal });
                        else if (meta.Value is float fVal)
                            payload.Add(meta.Key, new Value { DoubleValue = fVal }); // Qdrant thường dùng double cho float
                        else
                            payload.Add(meta.Key, new Value { StringValue = meta.Value?.ToString() ?? string.Empty });
                        // Chuyển đổi các loại dữ liệu sang Qdrant.Client.Grpc.Value
                       // payload.Add(meta.Key, new Value { StringValue = meta.Value?.ToString() ?? string.Empty });// Ví dụ: chuyển hết về string
                    }

                    points.Add(new PointStruct
                    {
                        Id = new PointId { Uuid = idsList[i] },
                        Vectors = new Vectors
                        {
                            Vector = new Vector { Data = { embeddingsList[i] } }
                        },
                        Payload = { payload }
                    });
                }

                // Upsert các điểm vào Collection
                await _qdrantClient.UpsertAsync(_qdrantCollectionName, points);

                Console.WriteLine($"Đã xử lý và lưu trữ {embeddingsList.Count} đoạn từ file '{file.FileName}' vào Qdrant.");
                return true;
            }


            return false;
        }

        // --- Helper methods for text extraction and chunking ---

        private string ExtractTextFromPdf(Stream pdfStream)
        {
            try
            {
                using (var document = PdfDocument.Open(pdfStream))
                {
                    var text = string.Join("\n", document.GetPages().Select(page => page.Text));
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        Console.WriteLine("PDF không chứa văn bản có thể trích xuất. Có thể cần OCR.");
                        return string.Empty;
                    }
                    return text;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi trích xuất nội dung từ PDF: {ex.Message}");
                return string.Empty;
            }
        }

        private string ExtractTextFromDocx(Stream docxStream)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(docxStream, false))
            {
                return wordDocument.MainDocumentPart.Document.Body.InnerText;
            }
        }

        private string ExtractTextFromXlsx(Stream xlsxStream)
        {
            using (var workbook = new XLWorkbook(xlsxStream))
            {
                var fullText = new System.Text.StringBuilder();
                foreach (var worksheet in workbook.Worksheets)
                {
                    fullText.AppendLine($"Worksheet: {worksheet.Name}");
                    foreach (var row in worksheet.RowsUsed())
                    {
                        foreach (var cell in row.CellsUsed())
                        {
                            fullText.Append(cell.GetText() + " "); // GetText() để lấy giá trị hiển thị
                        }
                        fullText.AppendLine();
                    }
                }
                return fullText.ToString();
            }
        }

        private List<string> ChunkText(string text, int chunkSize, int chunkOverlap)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();

            System.Text.StringBuilder currentChunk = new System.Text.StringBuilder();
            int currentLength = 0;

            foreach (var sentence in sentences)
            {
                if (currentLength + sentence.Length + 1 > chunkSize && currentLength > 0) // +1 for space/separator
                {
                    chunks.Add(currentChunk.ToString());
                    // Apply overlap: Start new chunk with a portion of the old chunk
                    currentChunk = new System.Text.StringBuilder(currentChunk.ToString().Substring(Math.Max(0, currentChunk.Length - chunkOverlap)));
                    currentLength = currentChunk.Length;
                }
                currentChunk.Append(sentence).Append(" ");
                currentLength += sentence.Length + 1;
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            return chunks;
        }
    }
}

