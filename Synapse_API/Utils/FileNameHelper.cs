﻿namespace Synapse_API.Utils
{
    public class FileNameHelper
    {
        public static string SetDocumentName(string userEmail, string fileName)
        {
            var ext = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);
            return $"doc/{userEmail}/{name}_{Guid.NewGuid()}{ext}";
        }

        public static string SetImageName(string userEmail, string fileName)        
        {
            var ext = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);
            return $"img/{userEmail}/{name}_{Guid.NewGuid()}{ext}";
        }
    }
}
