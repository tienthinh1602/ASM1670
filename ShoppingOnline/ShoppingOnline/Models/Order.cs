using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;

namespace ShoppingOnline.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public DateTime OrderTime { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public virtual Product? Product { get; set; }
    }
}
