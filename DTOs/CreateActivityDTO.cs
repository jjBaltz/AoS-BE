using AoS.Models;
namespace AoS.DTOs
{
    public class CreateActivityDTO
    {
        public string Description { get; set; }
        public string UID { get; set; }
        public List<int> TagIds { get; set; }
    }
}
