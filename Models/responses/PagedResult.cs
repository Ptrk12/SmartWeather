namespace Models.responses
{
    public class PagedResult<T>
    {
        public int TotalRecords { get; set; }
        public IEnumerable<T> Records { get; set; }
    }
}
