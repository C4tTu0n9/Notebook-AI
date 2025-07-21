namespace Synapse_API.Utils
{
    public class EmailHelper
    {
        public static async Task<string> LoadEmailTemplate(string templateName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Services", "AmazonServices", "EmailTemplates", templateName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Not found: {path}");
            }
            return await File.ReadAllTextAsync(path);
        }
    }
}
