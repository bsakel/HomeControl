using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HomeControl.AzureFunctions
{
    public class Function
    {

        [Function("Function")]
        public IActionResult HttpFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "functions/httpFunction")] HttpRequest req)
        {
            return new OkObjectResult($"Welcome to Azure Functions, {req.Query["name"]}!");
        }
    }
}
