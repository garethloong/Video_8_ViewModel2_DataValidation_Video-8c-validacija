using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Areas.ModulStudentskaSluzba.Data
{
    public class StudentUrediVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Ime je obavezno polje")]
        public string Ime { get; set; }
        [Required(ErrorMessage = "Prezime je obavezno polje")]
        public string Prezime { get; set; }
        //Range validator ne mozemo staviti jer nam je Broj indexa slozeni objekat (ima i slova i brojeve npr IB10029)
        //[Range(minimum: 1, maximum: 1200, ErrorMessage ="Broj indexa mora biti u opsegu 1 do 1200!")]

        //REGEX ima mjesto u ASP.NET ali je prije svega vezan za :NET framework
        //Vise o REGEX u .NET: https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices
        [RegularExpression(@"^[0-9A-Z]*$")] //ne dozvoljava mala slova, slovo Ž ili specijalne znake poput znaka +
        public string BrojIndexa { get; set; }
        public string Username { get; set; }
        [Required(ErrorMessage = "Password je obavezno polje")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Password mora imati najmanje 5 a najvise 30 karaktera")]
        public string Password { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DatumRodjenja { get; set; }
        public int SmjerId { get; set; }
        public List<SelectListItem> smjeroviStavke { get; set; }
    }
}