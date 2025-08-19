using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models
{
    public class FileUploadModel
    {
        [Required]
        [Display(Name = "Proof of payment")]
        public IFormFile ProofOfPayment { get; set; }

        [Display(Name = "Order ID")]
        public string? Orderid { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }
    }
}
