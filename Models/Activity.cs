namespace AoS.Models
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public bool IsUsed { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Memory> Memories { get; set; }
    }
}

