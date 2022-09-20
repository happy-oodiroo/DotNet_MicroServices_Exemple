using AutoMapper;

namespace DiplomaAndCertification.WebApi.Mapper
{
    public class DiplomaAndCertificationProfile: Profile
    {
        public DiplomaAndCertificationProfile()
        {
            CreateMap<Models.Entities.Certificate, DiplomaAndCertification.DTO.CertificateDto>().ReverseMap();
        }
    }
}
