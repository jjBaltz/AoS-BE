namespace AoS.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Label { get; set; }
        public List<Activity> Activities { get; set; }
    }
}