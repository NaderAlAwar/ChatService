namespace ChatService.Storage.Azure
{
    public class AzureStorageSettings
    {
        public string ConnectionString { get; set; }
        public string ProfilesTableName { get; set; }
        public string UsersTableName { get; set; }
    }
}