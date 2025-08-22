using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.CalendarReport
{
    public class OrderEventDto
    {
        public string OrderCode { get; set; }               // Mã đơn hàng (trích từ tiêu đề)
        public string Title { get; set; }                   // Tiêu đề đầy đủ
        public DateTime ?StartDate { get; set; }             // Ngày bắt đầu
        public DateTime? EndDate { get; set; }               // Ngày kết thúc
        public string Handler { get; set; }                 // Người xử lý (lấy từ organizer.displayName)
        public string QC { get; set; }                 // Người xử lý (lấy từ organizer.displayName)
        public string Sale { get; set; }                 // Người xử lý (lấy từ organizer.displayName)
        public List<TimelineStep> Timeline { get; set; }    // Các bước thực hiện (timeline)
    }

    public class TimelineStep
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }  // [v], [x], hoặc trống
        public bool IsOvertime { get; set; }
    }
}
