namespace Strona_do_rezerwacji_biletów.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int SeatsReserved { get; set; }
        public User User { get; set; }
        public Event Event { get; set; }
    }
}
