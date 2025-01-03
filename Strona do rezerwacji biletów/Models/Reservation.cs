namespace Strona_do_rezerwacji_biletów.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int EventId { get; set; }
        public int SeatsReserved { get; set; }
        public virtual Event Event { get; set; }
        public bool IsVIP { get; set; }
    }
}
