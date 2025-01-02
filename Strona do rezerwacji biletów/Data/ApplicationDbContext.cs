using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Strona_do_rezerwacji_biletów.Models;
using System;

namespace Strona_do_rezerwacji_biletów.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja dla encji Event i właściwości Date
            modelBuilder.Entity<Event>()
                .Property(e => e.Date)
                .HasConversion(new ValueConverter<DateTime, DateTime>(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc), // Konwersja podczas zapisu
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)  // Konwersja podczas odczytu
                ));
        }
    }
}
