// Models/AppointmentParticipant.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppointmentManager.Models
{
    public class AppointmentParticipant
    {
        public int Id { get; set; }
        
        public int AppointmentId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        public virtual Appointment? Appointment { get; set; }
    }
}
