using System;
using System.Collections.Generic;
using System.Text;

namespace DiplomaAndCertification.DTO
{
    public class CertificateDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string StudentUniqueIdentifier { get; set; }

        public string StudentFullName { get; set; }

        public DateTime ObtainedDate { get; set; }
    }
}
