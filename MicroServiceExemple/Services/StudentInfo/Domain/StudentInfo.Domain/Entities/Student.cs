using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentInfo.Domain.Entities
{
    public class Student : IdentityUser
    {
        
        public string? LastName  { get; set; }
        public string? FirstName { get; set; }
        public DateTime? Birthday { get; set; }

        public string? AddressLines { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }
}
