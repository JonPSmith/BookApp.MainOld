// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using BookApp.Main.LoggingServices;
using Microsoft.AspNetCore.Mvc;

namespace BookApp.Main.FrontEnd.Controllers
{
    public class LoggerController : Controller
    {
        [HttpGet]
        public JsonResult GetLog(string traceIdentifier)
        {
            return Json(HttpRequestLog.GetHttpRequestLog(traceIdentifier));
        }
    }
}