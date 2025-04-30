// Models/ViewModels/AppointmentViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentManager.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Appointment name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Set Reminder")]
        public int? ReminderMinutes { get; set; }

        [Display(Name = "Group Meeting")]
        public bool IsGroupMeeting { get; set; }

        public bool ReplaceExisting { get; set; }

        public bool IsOwner { get; set; }
    }
}
