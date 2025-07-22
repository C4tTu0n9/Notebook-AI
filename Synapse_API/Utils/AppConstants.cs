namespace Synapse_API.Utils
{
    public static class AppConstants
    {
        public static class Cache
        {
            public const int UserCacheMinutes = 10;
            public const int TokenCacheMinutes = 15;
        }

        public static class Validation
        {
            public const int MinPasswordLength = 6;
            public const int MaxPasswordLength = 100;
            public const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        }

        public static class ErrorMessages
        {
            public const string InvalidCredentials = "Incorrect email or password";
            public const string AccountInactive = "The account can no longer log in to the site due to a violation of community standards.";
            public const string EmailAlreadyExists = "Email already exists.";
            public const string FailedToCreateTopic = "Failed to create topic";
            public const string FailedToCreateQuiz = "Failed to create quiz";
            public const string TopicNotFound = "Topic not found";
            public const string QuizNotFound = "Quiz not found";
            public const string EventNotFound = "Event not found";
            public const string InvalidFileType = "Invalid file type";
            public const string Unauthorized = "You don't have permission to access this resource";
        }

        public static class DefaultValues
        {
            public const string DefaultStudyTime = "Evening";
            public const int DefaultStudyHours = 2;
            public const int DefaultReminderMinutes = 15;
        }
    }
} 