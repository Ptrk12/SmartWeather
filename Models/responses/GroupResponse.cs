namespace Models.responses
{
    public class GroupResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int NumberOfDevices { get; set; }
    }
}
