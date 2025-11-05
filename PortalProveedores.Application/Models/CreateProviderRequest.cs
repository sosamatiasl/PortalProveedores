using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalProveedores.Application.Models
{
    // DTO que usa un Cliente para registrar un Proveedor en su sistema
    public class CreateProviderRequest
    {
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string TaxId { get; set; } // CUIT, RUT, etc.
        [Required]
        public string ContactPerson { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
