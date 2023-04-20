namespace TWWAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Content { get; set; }
        public DateTime Published { get; set; }
    }
}
