using DocumentFormat.OpenXml.Vml;
using Microsoft.IdentityModel.Tokens;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Web;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Synapse_API.Models.Dto.ChatDTOs;

namespace Synapse_API.Services.AIServices
{
    public class ChatbotService
    {
        private readonly IGenerativeModelService _generativeModelService;
        private readonly QdrantClient _qdrantClient;
        private readonly string _qdrantCollectionName; 
        private readonly string _geminiEmbeddingModel;
        private readonly string _geminiGenerationModel;

        public ChatbotService(IGenerativeModelService generativeModelService, IConfiguration configuration, QdrantClient qdrantClient) // Inject QdrantClient
        {
            _generativeModelService = generativeModelService;
            _qdrantClient = qdrantClient;
            _qdrantCollectionName = configuration["Qdrant:CollectionName"] ?? "synapse-learning-knowledge-base";
            _geminiEmbeddingModel = configuration["Gemini:EmbeddingModel"] ?? "text-embedding-004";
            _geminiGenerationModel = configuration["Gemini:Model"] ?? "gemma-3-27b-it";
        }


        public async Task<ChatResponse> GetChatResponseAsync(ChatRequest request)
        {
            // 1. Tạo Embedding cho câu hỏi của người dùng (BẰNG GEMINI)
            var embeddingModel = _generativeModelService.CreateInstance(_geminiEmbeddingModel);
            //var questionContent = new Content(new List<Part> { new TextPart(request.Message) });
            var embedQuestionResponse = await embeddingModel.EmbedContent(request.Message);

            if (embedQuestionResponse?.Embedding?.Values?.Any() != true)
            {
                return new ChatResponse { Answer = "Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này. Vấn đề tạo embedding câu hỏi." };
            }

            var questionVector = embedQuestionResponse.Embedding.Values.ToList();


            // 2. Kiểm tra và tạo collection nếu chưa tồn tại
            var collections = await _qdrantClient.ListCollectionsAsync();
            if (!collections.Contains(_qdrantCollectionName))
            {
                await _qdrantClient.CreateCollectionAsync(
                    collectionName: _qdrantCollectionName,
                    vectorsConfig: new VectorParams { Size = 768, Distance = Distance.Cosine } // Giả định vector size 768
                );
                Console.WriteLine($"Created collection {_qdrantCollectionName}");
            }


            // 2. Truy vấn Qdrant để tìm các đoạn văn bản liên quan (THAY ĐỔI LỚN NHẤT Ở ĐÂY)
            var relevantChunks = new List<Models.Dto.ChatDTOs.RelevantChunk>();
            var sourceDocuments = new List<string>();

            // Xây dựng bộ lọc payload
            var filter = new Qdrant.Client.Grpc.Filter();
            if (request.UserId != -1)
            {
                filter.Must.Add(new Qdrant.Client.Grpc.Condition { Field = new FieldCondition { Key = "user_id", Match = new Match { Integer = request.UserId } } });
            }
            if (request.TopicId != -1)
            {
                filter.Must.Add(new Qdrant.Client.Grpc.Condition { Field = new FieldCondition { Key = "topic_id", Match = new Match { Integer = request.TopicId } } });
            }    
            if (request.CourseId != -1)
            {
                filter.Must.Add(new Qdrant.Client.Grpc.Condition { Field = new FieldCondition { Key = "course_id", Match = new Match { Integer = request.CourseId } } });
            }


            var searchResult = await _qdrantClient.SearchAsync(
                collectionName: _qdrantCollectionName,
                vector: questionVector.ToArray().AsMemory(),// Chuyển List<float> sang ReadOnlyMemory<float>
                limit: 3, // Lấy 3 kết quả hàng đầu
                filter: filter.Must.Any() ? filter : null, 
                payloadSelector: new WithPayloadSelector { Enable = true } // Yêu cầu trả về payload (metadata)
            );

            foreach (var foundPoint in searchResult)
            {
                if (foundPoint.Payload != null)
                {
                    // Lấy nội dung text từ payload (chúng ta đã lưu nó với key "text_content")
                    string docContent = foundPoint.Payload.ContainsKey("text_content") ? foundPoint.Payload["text_content"].StringValue : string.Empty;

                    // Xây dựng nguồn tài liệu từ metadata
                    string source = "Nguồn không xác định";
                    if (foundPoint.Payload.ContainsKey("original_filename"))
                    {
                        source = foundPoint.Payload["original_filename"].StringValue ?? source;
                    }
                    if (foundPoint.Payload.ContainsKey("document_name") && !string.IsNullOrEmpty(foundPoint.Payload["document_name"].StringValue))
                    {
                        source = foundPoint.Payload["document_name"].StringValue + (source != "Nguồn không xác định" ? $" ({source})" : "");
                    }
                    if (foundPoint.Payload.ContainsKey("chunk_index") && foundPoint.Payload["chunk_index"].HasIntegerValue)
                    {
                        source += $" (Đoạn: {foundPoint.Payload["chunk_index"].IntegerValue})";
                    }

                    if (!string.IsNullOrEmpty(docContent))
                    {
                        relevantChunks.Add(new Models.Dto.ChatDTOs.RelevantChunk { Content = docContent, Source = source });
                        if (!sourceDocuments.Contains(source))
                        {
                            sourceDocuments.Add(source);
                        }
                    }
                }
            }

            if (!relevantChunks.Any())
            {
                return new ChatResponse { Answer = "Tôi chưa tìm thấy thông tin cụ thể trong tài liệu. Bạn có thể hỏi rõ hơn không?" };
            }

            // 3. Xây dựng Prompt cho LLM (Giữ nguyên)
            var prompt = BuildPrompt(request.Message, relevantChunks, request.History);

            // 4. Gọi LLM để tạo câu trả lời (BẰNG GEMINI) 
            var generativeModel = _generativeModelService.CreateInstance(_geminiGenerationModel);

            //var chatContent = new Content(new List<Part> { new TextPart(prompt) });

            var generateContentResponse = await generativeModel.GenerateContent(prompt);

            if (generateContentResponse?.Text == null)
            {
                return new ChatResponse { Answer = "Xin lỗi, tôi gặp vấn đề khi tạo câu trả lời từ AI (Gemini)." };
            }

            var answer = generateContentResponse.Text;

            // 5. Cập nhật lịch sử trò chuyện
            var updatedHistory = new List<string>(request.History);
            updatedHistory.Add($"user: {request.Message}");
            updatedHistory.Add($"bot: {answer}");

            // Giới hạn lịch sử 
            if (updatedHistory.Count > 20)
            {
                updatedHistory = updatedHistory.Skip(updatedHistory.Count - 20).ToList();
            }

            return new ChatResponse
            {
                Answer = answer,
                SourceDocuments = sourceDocuments,
                History = updatedHistory
            };
        }

        private string BuildPrompt(string userQuestion, List<Models.Dto.ChatDTOs.RelevantChunk> chunks, List<string> history)
        {
            var context = string.Join("\n\n---\n\n", chunks.Select(c => $"Nội dung: {c.Content}\nNguồn: {c.Source}")); var historyString = string.Empty;
            if (history != null && history.Any())
            {
                historyString = "\n\nLịch sử trò chuyện:\n" + string.Join("\n", history);
            }

            return $"Thông tin tham khảo:\n{context}{historyString}\n\nCâu hỏi hiện tại: {userQuestion}\n\nTrả lời dựa trên thông tin tham khảo và lịch sử trò chuyện. Nếu thông tin không đủ, hãy nói rõ.";
        }
    }
}

