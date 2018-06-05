﻿using MediatR;
using SFA.DAS.AssessorService.Domain.Entities;

namespace SFA.DAS.AssessorService.Api.Types.Models.Certificates
{
    public class UpdateCertificateRequest : IRequest<Certificate>
    {
        public string Username { get; set; }
        public Certificate Certificate { get; }
        public string Action { get; set; }

        public UpdateCertificateRequest(Certificate certificate)
        {
            Certificate = certificate;
        }
    }
}