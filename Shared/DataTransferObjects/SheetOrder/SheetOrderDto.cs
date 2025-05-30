namespace Shared.DataTransferObjects.SheetOrder
{
    public record SheetOrderDto
    {
        public string OrderCode { get; init; }
        public string? OrderName { get; init; }
        public string? Dev1 { get; init; }
        public string? QC { get; init; }
        public string? Code { get; init; }
        public string? Sale { get; init; }
        public DateTime? EndDate { get; init; }
        public string? Status { get; init; }
        public string? SheetName { get; init; }
    }
}