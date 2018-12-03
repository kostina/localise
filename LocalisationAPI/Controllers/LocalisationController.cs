using Localisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using LocalisationAPI.Request;

namespace LocalisationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalisationController : ControllerBase
    {
        private ILogger _logger;
        public LocalisationController(LocaliseClient client,  ILogger logger)
        {
            _localiseClient = client;
            _logger = logger;
        }

        private LocaliseClient _localiseClient;

        [HttpGet("{scope}/{slug}/{iso}")]
        public ActionResult<string> Get(string iso, string scope, string slug)
        {
            try
            {
                var localised = _localiseClient.Cache.GetTranslation(iso, slug);
                return new JsonResult(new { localised, slug }) { StatusCode = (int)HttpStatusCode.OK };
            }
            catch (LocalisationNotFoundException ex)
            {
                return new ContentResult()
                { Content = ex.Message, StatusCode = (int)HttpStatusCode.BadRequest };
            }
        }

        [HttpPost]
        [Route("processlist")]
        public ActionResult<string> ProcessList(IEnumerable<LocalisationRequest> request)
        {
            var result = new List<LocalisationResponse>();
            foreach (var localisationRequest in request)
            {
                try
                {
                    var localised = _localiseClient.Cache.GetTranslation(localisationRequest.ISO, localisationRequest.Slug);
                    result.Add(new LocalisationResponse() {Localised = localised, Slug = localisationRequest.Slug});
                }
                catch (LocalisationNotFoundException ex)
                {
                    _logger.Log(LogLevel.Warning, ex , string.Format("Localisation not found"));
                }
            }
            return new JsonResult(result) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}