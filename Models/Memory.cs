namespace AoS.Models
{
    public class Memory
    {
        public int MemoryId { get; set; }
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
    }
}

