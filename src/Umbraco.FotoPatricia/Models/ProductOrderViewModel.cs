using System.ComponentModel.DataAnnotations;

namespace Umbraco.FotoPatricia.Models;

public class ProductOrderViewModel : ContactFormViewModel
{
    public IReadOnlyCollection<ProductOrderModel> Products { get; }

    [Required] 
    [Display(Name = "Produkt")]
    public string Product { get; set; } = string.Empty;
    
    [Required] 
    [Display(Name = "Telefon / Mobile")]
    public override string PhoneNumber { get; set; } = string.Empty;

    [Required] 
    [Display(Name = "Adresse")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Wunschtermin 1")]
    public string PreferredWeekday1 { get; set; } = string.Empty;

    [Display(Name = "Wunschtermin 2")]
    public string? PreferredWeekday2 { get; set; }

    public ProductOrderViewModel(IReadOnlyCollection<ProductOrderModel> products)
    {
        Products = products;
    }

    public ProductOrderViewModel()
    {
        Products = new List<ProductOrderModel>();
    }
}

public class ProductOrderModel
{
    public string Name { get; set; } = string.Empty;

    public string Price { get; set; } = string.Empty;
}