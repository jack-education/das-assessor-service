﻿namespace SFA.DAS.AssessorService.Application.Api.Client
{
    public interface ITokenService
    {
        string GetToken();
    }
    
    public interface IQnaTokenService : ITokenService
    {
    }
}