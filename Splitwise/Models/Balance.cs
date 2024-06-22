namespace Splitwise.Models
{
    public class Balance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Person> Persons { get; set; } = new List<Person>();
    }
}
