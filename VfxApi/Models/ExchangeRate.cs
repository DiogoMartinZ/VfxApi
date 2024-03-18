namespace VfxApi.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public string? CurrencyPairFrom { get; set; }
        public string? CurrencyPairTo { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}
