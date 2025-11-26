using System.Text.Json.Serialization;

namespace WEB.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        
        [JsonIgnore]
        public Category Category { get; set; }

    }
}
