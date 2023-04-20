namespace TWWAPI.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string GoogleMapsCoordinates { get; set; }
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public virtual List<Comment>? Comments { get; set; }
    }
}
