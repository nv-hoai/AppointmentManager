// Models/ViewModels/CalendarViewModel.cs
using System;
using System.Collections.Generic;

namespace AppointmentManager.Models.ViewModels
{
    public class CalendarViewModel
    {
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
