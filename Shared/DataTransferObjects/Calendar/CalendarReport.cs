using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.CalendarReport
{
    public record class CalendarReport
    {
        public string Title { get; init; }
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public DateTime QCDay { get; init; }
        public int EventDays { get; init; }
        public int Stars { get; init; }
        public int OvertimeDays { get; set; }
        public decimal PC { get; set; }
        public List<DateTime> OvertimeDates { get; set; } = new List<DateTime>();
    }
}
