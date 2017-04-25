using System.ComponentModel.DataAnnotations;

namespace TestAPI.Models
{
    public class Users
    {
        [Key]
        public int ID { get; set; }
        public string name { get; set; }
        public string pwd { get; set; }
    }
    
}
