using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaAndCertification.Domain.Entities
{
    public class Certificate
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string StudentUniqueIdentifier { get; set; }

        public DateTime ObtainedDate { get; set; }

    }
}
