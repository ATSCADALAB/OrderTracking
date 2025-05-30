namespace Shared.DataTransferObjects.CalendarReport
{
    public class UnifiedOrderDto
    {
        public string OrderCode { get; set; } // Mã đơn hàng (chung)
        public string Title { get; set; } // Tiêu đề hoặc tên đơn hàng (chung)
        public DateTime? StartDate { get; set; } // Ngày bắt đầu (từ Calendar)
        public DateTime? EndDate { get; set; } // Ngày kết thúc hoặc hoàn thành (chung)
        public string Handler { get; set; } // Người xử lý (từ Calendar hoặc Sale từ Sheets)
        public string Source { get; set; } // Nguồn dữ liệu: "Calendar", "Sheets", hoặc "Both"
        public string Status { get; set; } // Trạng thái (từ Sheets)
        public List<TimelineStep> Timeline { get; set; } // Timeline (từ Calendar)
        public string Dev1 { get; set; } // KTV (từ Sheets)
        public string QC { get; set; } // QC (từ Sheets)
        public string Code { get; set; } // Code (từ Sheets)
        public string Sale { get; set; } // Sale (từ Sheets)
    }
}