namespace AoS.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName{ get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public List <Activity>? Activities { get; set; }
        public List <Memory>? Memories { get; set; }
    }
}