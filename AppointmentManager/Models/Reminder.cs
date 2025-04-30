// Models/Reminder.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentManager.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        
        public int AppointmentId { get; set; }
        
        [Required]
        public DateTime ReminderTime { get; set; }
        
        public bool IsNotified { get; set; }
        
        public virtual Appointment? Appointment { get; set; }
    }
}
