namespace Strona_do_rezerwacji_biletów.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int AvailableNormalSeats { get; set; }
        public int AvailableVIPSeats { get; set; }
        public string ImagePath { get; set; }
    }
}
