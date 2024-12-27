using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Strona_do_rezerwacji_biletów.Data;
using Strona_do_rezerwacji_biletów.Models;
using System;
using System.Security.Claims;

namespace Strona_do_rezerwacji_biletów.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var events = _context.Events.ToList();
            return View(events);
        }

        public IActionResult Reserve(int id)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();

            return View(ev);
        }
        public IActionResult Details(int id)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }

        [HttpPost]
        public IActionResult Reserve(int eventId, int seatsReserved)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == eventId);
            if (ev == null || ev.AvailableSeats < seatsReserved) return BadRequest("Not enough seats available.");

            var reservation = new Reservation
            {
                EventId = eventId,
                UserId = User.Identity.Name,
                SeatsReserved = seatsReserved
            };

            ev.AvailableSeats -= seatsReserved;
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult MyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .ToList();
            if (reservations == null)
            {
                return NotFound();
            }

            return View(reservations);
        }

        [HttpPost]
        public IActionResult CancelReservation(int id)
        {
            var reservation = _context.Reservations.Find(id);

            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                _context.SaveChanges();
            }

            return RedirectToAction("MyReservations");
        }
    }
}
