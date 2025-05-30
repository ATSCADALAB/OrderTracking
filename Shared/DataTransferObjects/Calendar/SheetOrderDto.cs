using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Calendar
{
    public class SheetOrderDto
    {
        public string OrderCode { get; set; }           // Mã đơn hàng (ví dụ: D1394)
        public string OrderName { get; set; }           // Tên/mô tả đơn hàng
        public string Dev1 { get; set; }                // KTV(Dev1)
        public string QC { get; set; }                  // QC(Dev2)
        public string Code { get; set; }                // CODE
        public string Sale { get; set; }                // SALE
        public DateTime? EndDate { get; set; }          // Ngày hoàn thành
        public string Status { get; set; }              // Trạng thái từ màu nền: Bình thường / Cận ngày / Trễ ngày
    }
}
