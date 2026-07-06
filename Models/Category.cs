namespace NeighborHelp.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public virtual ICollection<HelpRequest> HelpRequests { get; set; } = new List<HelpRequest>();
    }
}
