namespace AdminService.Infrastructure.Sync
{
    public class AdminSyncOptions
    {
        public bool Enabled { get; set; } = true;
        public bool RunOnStartup { get; set; } = true;
        public int IntervalMinutes { get; set; } = 30;
        public SyncMongoSource JobSeeker { get; set; } = new();
        public SyncMongoSource JobProvider { get; set; } = new();
    }

    public class SyncMongoSource
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
