namespace Synapse_API.Services.DocumentServices.Interfaces
{
    public interface IDocumentProcessingService
    {
        Task<bool> ProcessAndEmbedDocumentAsync(IFormFile file, int userId, string documentName, int courseId, int topicId);
    }
}
