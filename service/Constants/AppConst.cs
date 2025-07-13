namespace WhatsAppCampaignManager.Constants
{
    public static class AppConst
    {
        public static class JobStatus
        {
            public const string PENDING = "Pending";
            public const string RUNNING = "Running";
            public const string COMPLETED = "Completed";
            public const string FAILED = "Failed";
            public const string CANCELLED = "Cancelled";
        }

        public static class JobSettings
        {
            public const int MAX_CONCURRENT_JOBS_PER_USER = 2;
            public const int MAX_USERS_PROCESSING = 10;
            public const int JOB_PROCESSING_INTERVAL_MINUTES = 1;
        }
    }
}
