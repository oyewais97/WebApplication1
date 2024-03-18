namespace WebApplication1.Model;
using System.ComponentModel.DataAnnotations;
public class Token
    {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    public decimal TotalSupply { get; set; }

    [Required]
    public decimal CirculatingSupply { get; set; }

    [Required]
    [StringLength(42)]
    public string Address { get; set; }

    [Required]
    public decimal totalAmount { get; set; }
}
  