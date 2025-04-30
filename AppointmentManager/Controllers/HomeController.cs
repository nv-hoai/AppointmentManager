using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppointmentManager.Data;
using AppointmentManager.Models;
using AppointmentManager.Models.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppointmentManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var appointments = await _context.Appointments
                .Include(a => a.Participants)
                .Where(a => a.UserId == user.Id ||
                            a.Participants.Any(p => p.UserId == user.Id))
                .ToListAsync();

            var viewModel = new CalendarViewModel
            {
                Appointments = appointments
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create(DateTime date)
        {
            if (date == default)
            {
                date = DateTime.Today;
            }

            var model = new AppointmentViewModel
            {
                Date = date.Date,
                StartTime = date.Date.AddHours(DateTime.Now.Hour + 1),
                EndTime = date.Date.AddHours(DateTime.Now.Hour + 2)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var startDateTime = CombineDateAndTime(model.Date, model.StartTime);
                var endDateTime = CombineDateAndTime(model.Date, model.EndTime);

                if (endDateTime <= startDateTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time.");
                    return View(model);
                }

                if (!model.ReplaceExisting)
                {
                    var conflicts = await CheckForConflicts(user.Id, startDateTime, endDateTime);
                    if (conflicts.Any())
                    {
                        ViewBag.HasConflicts = true;
                        ViewBag.Conflicts = conflicts;
                        return View(model);
                    }
                }

                var similarMeetings = await CheckForSimilarMeetings(model.Name, startDateTime, endDateTime);
                if (similarMeetings.Any() && !model.ReplaceExisting)
                {
                    ViewBag.SimilarMeetings = similarMeetings;
                    return View(model);
                }

                var appointment = new Appointment
                {
                    Name = model.Name,
                    Location = model.Location,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    UserId = user.Id,
                    IsGroupMeeting = model.IsGroupMeeting
                };

                if (model.ReminderMinutes.HasValue && model.ReminderMinutes.Value > 0)
                {
                    var reminder = new Reminder
                    {
                        ReminderTime = startDateTime.AddMinutes(-model.ReminderMinutes.Value),
                        IsNotified = false
                    };

                    appointment.Reminders.Add(reminder);
                }

                if (model.ReplaceExisting)
                {
                    TempData["Warning"] = "Manually remove the overlapping appointments if needed";
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .Include(a => a.Reminders)
                .FirstOrDefaultAsync(a => a.Id == id && (a.UserId == user.Id ||
                            a.Participants.Any(p => p.UserId == user.Id)));

            if (appointment == null)
            {
                return NotFound();
            }

            var viewModel = new AppointmentViewModel
            {
                Id = appointment.Id,
                Name = appointment.Name,
                Location = appointment.Location,
                Date = appointment.StartTime.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                IsGroupMeeting = appointment.IsGroupMeeting,
                IsOwner = appointment.UserId == user.Id,
            };

            var reminder = appointment.Reminders.FirstOrDefault();
            if (reminder != null)
            {
                var reminderMinutes = (int)(appointment.StartTime - reminder.ReminderTime).TotalMinutes;
                viewModel.ReminderMinutes = reminderMinutes;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);

                    var appointment = await _context.Appointments
                        .Include(a => a.Reminders)
                        .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    var startDateTime = CombineDateAndTime(model.Date, model.StartTime);
                    var endDateTime = CombineDateAndTime(model.Date, model.EndTime);

                    if (endDateTime <= startDateTime)
                    {
                        ModelState.AddModelError("EndTime", "End time must be after start time.");
                        return View(model);
                    }

                    appointment.Name = model.Name;
                    appointment.Location = model.Location;
                    appointment.StartTime = startDateTime;
                    appointment.EndTime = endDateTime;
                    appointment.IsGroupMeeting = model.IsGroupMeeting;

                    if (model.ReminderMinutes.HasValue && model.ReminderMinutes.Value > 0)
                    {
                        var reminder = appointment.Reminders.FirstOrDefault();
                        if (reminder == null)
                        {
                            reminder = new Reminder
                            {
                                ReminderTime = startDateTime.AddMinutes(-model.ReminderMinutes.Value),
                                IsNotified = false
                            };
                            appointment.Reminders.Add(reminder);
                        }
                        else
                        {
                            reminder.ReminderTime = startDateTime.AddMinutes(-model.ReminderMinutes.Value);
                            reminder.IsNotified = false;
                        }
                    }
                    else
                    {
                        appointment.Reminders.Clear();
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> JoinMeeting(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .Include(a => a.Participants)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsGroupMeeting);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.UserId == user.Id || appointment.Participants.Any(p => p.UserId == user.Id))
            {
                TempData["Error"] = "You are already a participant in this meeting.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Participants.Add(new AppointmentParticipant
            {
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "You have successfully joined the meeting.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> LeaveMeeting(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .Include(a => a.Participants)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsGroupMeeting);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Participants.Any(p => p.UserId == user.Id))
            {
                TempData["Success"] = "You have successfully leaved the meeting.";
                var participant = appointment.Participants.First(p => p.UserId == user.Id);
                _context.AppointmentParticipants.Remove(participant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "There's an error when leaving the meeting.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckConflicts(DateTime start, DateTime end)
        {
            var user = await _userManager.GetUserAsync(User);

            var conflicts = await CheckForConflicts(user.Id, start, end);

            return Json(new { hasConflicts = conflicts.Any(), conflicts });
        }

        [HttpGet]
        public async Task<IActionResult> CheckSimilarMeetings(string name, DateTime start, DateTime end)
        {
            var similarMeetings = await CheckForSimilarMeetings(name, start, end);

            return Json(new { hasSimilar = similarMeetings.Any(), similarMeetings });
        }

        private async Task<IList<Appointment>> CheckForConflicts(string userId, DateTime start, DateTime end)
        {
            return await _context.Appointments
                .Include(a => a.Participants)
                .Where(a => (a.UserId == userId ||
                             a.Participants.Any(p => p.UserId == userId)) &&
                            ((a.StartTime <= start && a.EndTime > start) ||
                             (a.StartTime < end && a.EndTime >= end) ||
                             (a.StartTime >= start && a.EndTime <= end)))
                .ToListAsync();
        }

        private async Task<IList<Appointment>> CheckForSimilarMeetings(string name, DateTime start, DateTime end)
        {
            var lowerName = name.ToLower();

            return await _context.Appointments
                .Where(a => a.IsGroupMeeting &&
                            EF.Functions.Like(a.Name.ToLower(), lowerName) && 
                            a.StartTime >= start.AddMinutes(-30) &&           
                            a.StartTime <= start.AddMinutes(30) &&
                            a.EndTime >= end.AddMinutes(-30) &&
                            a.EndTime <= end.AddMinutes(30))
                .ToListAsync();
        }

        private DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(
                date.Year,
                date.Month,
                date.Day,
                time.Hour,
                time.Minute,
                0
            );
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
