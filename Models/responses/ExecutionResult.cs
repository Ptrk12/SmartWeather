namespace Models.responses
{
    public class ExecutionResult
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}
