namespace DataModels
{
    public class AccountUpdateMessage
    {
        public string? AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionType { get; set; } // "DEPOSIT", "WITHDRAWAL", "INTEREST", "FEE"
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ReferenceId { get; set; }
    }
}
