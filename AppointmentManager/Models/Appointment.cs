// Models/Appointment.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppointmentManager.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Appointment name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required.")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Created By")]
        public string UserId { get; set; } = string.Empty;

        public bool IsGroupMeeting { get; set; }

        public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
        
        public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}
