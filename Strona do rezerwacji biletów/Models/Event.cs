namespace Strona_do_rezerwacji_biletów.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int AvailableSeats { get; set; }
    }
}
