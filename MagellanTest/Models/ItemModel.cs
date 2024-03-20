using System.ComponentModel.DataAnnotations;

namespace MagellanTest.Models
{
    public class ItemModel
    {
        [Key]
        public int id {get; set;}
        [Required]
        public string item_name {get; set;}
        public int? parent_item {get; set;}
        [Required]
        public int cost {get; set;}
        [Required]
        public string req_date {get; set;}
    }
}