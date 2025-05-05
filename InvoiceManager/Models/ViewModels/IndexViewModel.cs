using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace InvoiceManager.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }

        [Column(TypeName = "varchar")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }
}