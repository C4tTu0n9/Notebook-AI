namespace Synapse_API.Models.Dto.ChatDTOs
{
    public class RelevantChunk
    {
        public string Content { get; set; } // Nội dung văn bản của đoạn tìm thấy
        public string Source { get; set; }  // Nguồn gốc của đoạn (ví dụ: "TenFile.pdf (Đoạn: 5)")
    }
}
