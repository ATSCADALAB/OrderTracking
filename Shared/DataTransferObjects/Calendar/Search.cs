namespace Shared.DataTransferObjects.CalendarReport 
{
    public class OrderSearchResultDto {
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime QCDay { get; set; }
        public int EventDays { get; set; }
        public int Stars { get; set; }
    }
}