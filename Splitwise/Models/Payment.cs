using System.ComponentModel.DataAnnotations.Schema;

namespace Splitwise.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
    }
}
