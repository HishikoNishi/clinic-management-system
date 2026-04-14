namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class DoctorWorkSummaryDto
    {
        public DateTime ReferenceDate { get; set; }
        public int SelectedDateSlots { get; set; }
        public int SelectedDateMinutes { get; set; }
        public int TodaySlots { get; set; }
        public int TodayMinutes { get; set; }
        public int CurrentMonthSlots { get; set; }
        public int CurrentMonthMinutes { get; set; }
        public int CurrentYearSlots { get; set; }
        public int CurrentYearMinutes { get; set; }
    }
}
