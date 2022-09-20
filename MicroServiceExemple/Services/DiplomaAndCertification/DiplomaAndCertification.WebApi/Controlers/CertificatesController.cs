using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomaAndCertification.WebApi.Models.Entities;
using DiplomaAndCertification.WebApi.Models.Persistence;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
using MassTransit;
using EventBus.Messages.Events;

namespace DiplomaAndCertification.WebApi.Controlers
{
    [Route("DiplomaAndCertification/api/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificatesController : ControllerBase
    {
        private readonly DiplomaAndCertificationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public CertificatesController(DiplomaAndCertificationDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        // GET: DiplomaAndCertification/api/Certificates
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(IEnumerable<DTO.CertificateDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "NotFound")]
        public async Task<ActionResult<IEnumerable<DTO.CertificateDto>>> GetCertificates()
        {
            var studentUniqueIdentifier=User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var studentFullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (_context.Certificates == null)
            {
                return NotFound();
            }
            return await _context.Certificates.Where(x=>x.StudentUniqueIdentifier==studentUniqueIdentifier).
                Select(x=> new DTO.CertificateDto() {
                    Id=x.Id,
                    Title = x.Title,
                    StudentUniqueIdentifier =x.StudentUniqueIdentifier, 
                    StudentFullName= studentFullName,
                    ObtainedDate =x.ObtainedDate
                })
                .ToListAsync();
        }

        // GET: DiplomaAndCertification/api/Certificates/5
        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(DTO.CertificateDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "NotFound")]
        public async Task<ActionResult<DTO.CertificateDto>> GetCertificate(Guid id)
        {
            var studentUniqueIdentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var studentFullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (_context.Certificates == null)
            {
                return NotFound();
            }
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null||certificate.StudentUniqueIdentifier!=studentUniqueIdentifier)
            {
                return NotFound();
            }
            var returnedObject = _mapper.Map< DTO.CertificateDto>(certificate);
            returnedObject.StudentFullName = studentFullName;
            return returnedObject;
        }

        // PUT: api/Certificates/5
        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "NotFound")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "BadRequest")]
        public async Task<IActionResult> PutCertificate(Guid id, DTO.CertificateDto certificateDto)
        {
            if (id != certificateDto.Id)
            {
                return BadRequest();
            }
            Certificate certificate = _mapper.Map<Certificate>(certificateDto);
            _context.Entry(certificate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CertificateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Certificates
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(DTO.CertificateDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "NotFound")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "InternalServerError",typeof(DTO.MessageResponse))]
        public async Task<ActionResult<DTO.CertificateDto>> PostCertificate(DTO.CertificateDto certificateDto)
        {
            if (_context.Certificates == null)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,new DTO.MessageResponse(  "Entity set 'DiplomaAndCertificationDbContext.Certificates'  is null."));
            }
            Certificate certificate = _mapper.Map<Certificate>(certificateDto);
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();
            certificateDto.Id = certificate.Id;
            return CreatedAtAction("GetCertificate", new { id = certificate.Id }, certificateDto);
        }

        // DELETE: api/Certificates/5
        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "NotFound")]
        public async Task<IActionResult> DeleteCertificate(Guid id)
        {
            if (_context.Certificates == null)
            {
                return NotFound();
            }
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }

            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CertificateExists(Guid id)
        {
            return (_context.Certificates?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Post: api/Certificates/SendItByEmail/5
        [HttpPost("SendItByEmail/{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Success",typeof(DTO.MessageResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "NotFound")]
        public async Task<IActionResult> SendItByEmail(Guid id)
        {
            var studentUniqueIdentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var studentFullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var studentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (_context.Certificates == null)
            {
                return NotFound();
            }
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null || certificate.StudentUniqueIdentifier != studentUniqueIdentifier)
            {
                return NotFound();
            }
            var returnedObject = _mapper.Map<DTO.CertificateDto>(certificate);
            returnedObject.StudentFullName = studentFullName;

            //Send "Send Email Event to rabbitmq
            var eventMessage = new SendEmailEvent()
            {
                To = new List<string>() { studentEmail },
                Subject = $"{certificate.Title} Certificate",
                Body = $"This email represents a certificate for {certificate.Title}\r\nobtained by {studentFullName} on {certificate.ObtainedDate.ToShortTimeString}",
                IsBodyHtml=false
            };
            await _publishEndpoint.Publish<SendEmailEvent>(eventMessage);
            return StatusCode(StatusCodes.Status202Accepted, new DTO.MessageResponse("You will receive an email with the certification as soon as possible."));
        }

    }
}
