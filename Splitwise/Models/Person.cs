namespace Splitwise.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double BalanceTotal { get; set; } = 0;
        public int BalanceId { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }
}
