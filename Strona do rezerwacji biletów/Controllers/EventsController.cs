﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
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
        public IActionResult Index(string category, string searchQuery)
        {
            var categories = _context.Events
                .Select(e => e.Category)
                .Distinct()
                .ToList();

            var events = _context.Events.AsQueryable();

            // Filtrowanie po kategorii
            if (!string.IsNullOrEmpty(category))
            {
                events = events.Where(e => e.Category == category);
            }

            // Wyszukiwanie po tytule i opisie
            if (!string.IsNullOrEmpty(searchQuery))
            {
                events = events.Where(e => e.Title.Contains(searchQuery) || e.Description.Contains(searchQuery));
            }

            // Pobranie wyników i sortowanie po dacie
            var sortedEvents = events.OrderBy(e => e.Date).ToList();

            // Przekazanie danych do widoku
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.SearchQuery = searchQuery;

            return View(sortedEvents);
        }

        /*public IActionResult Index()
        {
            var events = _context.Events.ToList();
            return View(events);
        }

        /*public IActionResult Reserve(int id)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();

            return View(ev);
        }*/
        [HttpGet]
        public IActionResult Details(int id)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }

        [HttpGet]
        public IActionResult Reserve(int id)
        {
            // Znajdź wydarzenie w bazie danych
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                return NotFound();
            }

            // Przekaż dane do widoku
            return View(ev);
        }

        [HttpPost]
        public IActionResult Reserve(int eventId, int seatsReserved, bool VIPticket)
        {
            // Znajdź wydarzenie w bazie danych
            var ev = _context.Events.FirstOrDefault(e => e.Id == eventId);
            if (ev == null)
            {
                return NotFound();
            }

            // Sprawdź dostępność miejsc
            var seatsAvailable= VIPticket ? ev.AvailableVIPSeats : ev.AvailableNormalSeats;
            
            if (seatsReserved > seatsAvailable)
            {
                ModelState.AddModelError("", "Not enough seats available.");
                return View(ev);
            }

            // Pobierz ID zalogowanego użytkownika
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Twórz rezerwację
            var reservation = new Reservation
            {
                EventId = eventId,
                UserId = userId,
                SeatsReserved = seatsReserved,
                IsVIP = VIPticket,
            };

            // Zmniejsz liczbę dostępnych miejsc
            if (reservation.IsVIP)
            { ev.AvailableVIPSeats -= seatsReserved; }
            else { ev.AvailableNormalSeats -= seatsReserved; }

                
            

            // Zapisz rezerwację i zaktualizuj wydarzenie
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            // Przekieruj użytkownika do strony potwierdzenia lub listy wydarzeń
            return RedirectToAction("Index");
        }

        /*[HttpPost]
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
        }*/
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
            // Znajdź rezerwację na podstawie jej ID
            var reservation = _context.Reservations.Find(id);

            if (reservation != null)
            {
                // Znajdź powiązane wydarzenie
                var ev = _context.Events.FirstOrDefault(e => e.Id == reservation.EventId);

                if (ev != null)
                {
                    // Powiększ liczbę dostępnych miejsc
                    if (reservation.IsVIP)
                    { ev.AvailableVIPSeats += reservation.SeatsReserved; }
                    else { ev.AvailableNormalSeats += reservation.SeatsReserved; }
                }

                // Usuń rezerwację z bazy danych
                _context.Reservations.Remove(reservation);

                // Zapisz zmiany w bazie danych
                _context.SaveChanges();
            }

            // Przekieruj użytkownika do strony z jego rezerwacjami
            return RedirectToAction("MyReservations");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            return View();
        }
        [Authorize(Roles ="Admin") ]
        [HttpPost]
        public IActionResult Add(EventCreate model)
        {
            if (ModelState.IsValid)
            {
                // Ustaw ścieżkę folderu, w którym zapiszesz plik
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events");

                // Upewnij się, że folder istnieje
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Generuj unikalną nazwę pliku
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                // Pełna ścieżka do zapisu
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Zapis pliku
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                // Tworzenie zmiennej ścieżki
                string imagePath = $"/images/events/{uniqueFileName}";
                Event ev = new Event
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Date = model.Date,
                    Category = model.Category,
                    AvailableNormalSeats = model.AvailableNormalSeats,
                    AvailableVIPSeats = model.AvailableVIPSeats,
                    ImagePath = imagePath,
                    CreatorId = userId
                };

                _context.Events.Add(ev);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else if (model.Image == null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                Event ev = new Event
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Date = model.Date,
                    Category = model.Category,
                    AvailableNormalSeats = model.AvailableNormalSeats,
                    AvailableVIPSeats = model.AvailableVIPSeats,
                    ImagePath = "",
                    CreatorId = userId
                };
                _context.Events.Add(ev);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var eventToDelete = _context.Events.Find(id);
            if (eventToDelete == null)
            {
                return NotFound();
            }
            return View(eventToDelete);
        }

        // POST: Events/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var eventToDelete = _context.Events
                                        .Include(e => e.Reservations) // Załadowanie rezerwacji powiązanych z wydarzeniem
                                        .FirstOrDefault(e => e.Id == id);

            if (eventToDelete == null)
            {
                return NotFound();
            }

            // Usunięcie rezerwacji związanych z wydarzeniem
            var reservationsToDelete = eventToDelete.Reservations.ToList();
            _context.Reservations.RemoveRange(reservationsToDelete);

            // Usunięcie wydarzenia
            _context.Events.Remove(eventToDelete);

            // Zapisanie zmian w bazie danych
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Edit(EventCreate model)
        {
            if (ModelState.IsValid)
            {
                // Ustaw ścieżkę folderu, w którym zapiszesz plik
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events");

                // Upewnij się, że folder istnieje
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Generuj unikalną nazwę pliku
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                // Pełna ścieżka do zapisu
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Zapis pliku
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                // Tworzenie zmiennej ścieżki
                string imagePath = $"/images/events/{uniqueFileName}";
                Event ev = new Event
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Date = model.Date,
                    Category = model.Category,
                    AvailableNormalSeats = model.AvailableNormalSeats,
                    AvailableVIPSeats = model.AvailableVIPSeats,
                    ImagePath = imagePath,
                    CreatorId = userId
                };

                _context.Events.Add(ev);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else if (model.Image == null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                Event ev = new Event
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Date = model.Date,
                    Category = model.Category,
                    AvailableNormalSeats = model.AvailableNormalSeats,
                    AvailableVIPSeats = model.AvailableVIPSeats,
                    ImagePath = "",
                    CreatorId = userId
                };
                _context.Events.Add(ev);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
