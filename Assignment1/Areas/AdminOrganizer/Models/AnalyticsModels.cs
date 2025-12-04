namespace Assignment1.Areas.AdminOrganizer.Models
{
    // Tickets sold per category
    public class TicketSalesByCategoryModel
    {
        public string Category { get; set; } = string.Empty;
        public int TicketsSold { get; set; } = 0;
    }

    // Revenue per month
    public class RevenuePerMonthModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; } = 0m;
    }

    // Top 5 best-selling events
    public class TopEventModel
    {
        public string Name { get; set; } = string.Empty;
        public int TicketsSold { get; set; } = 0;
        public decimal Revenue { get; set; } = 0m;
    }
}