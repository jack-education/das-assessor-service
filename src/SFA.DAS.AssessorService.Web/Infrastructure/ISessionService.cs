﻿using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Web.Controllers;

namespace SFA.DAS.AssessorService.Web.Infrastructure
{
    public interface ISessionService
    {
        void Set(string key, object value);
        void Set(string key, string stringValue);
        void Remove(string key);
        string Get(string key);
        T Get<T>(string key);
    }

    class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _environment;

        public SessionService(IHttpContextAccessor httpContextAccessor, string environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }
        
        public void Set(string key, object value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(_environment + "_" + key,
                JsonConvert.SerializeObject(value));
        }

        public void Set(string key, string stringValue)
        {
            _httpContextAccessor.HttpContext.Session.SetString(_environment + "_" + key,
                stringValue);
        }

        public void Remove(string key)
        {
            _httpContextAccessor.HttpContext.Session.Remove(_environment + "_" + key);
        }

        public string Get(string key)
        {
            return _httpContextAccessor.HttpContext.Session.GetString(_environment + "_" + key);
        }

        public T Get<T>(string key)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            key = _environment + "_" + key;

            if (session.Keys.All(k => k != key))
            {
                return default(T);
            }

            var value = session.GetString(key);

            return string.IsNullOrWhiteSpace(value) ? default(T): JsonConvert.DeserializeObject<T>(value);
        }
    }
}