// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using BookApp.Main.LoggingServices;
using Microsoft.AspNetCore.Mvc;

namespace BookApp.Main.FrontEnd.Controllers
{
    public abstract class BaseTraceController : Controller
    {
        protected void SetupTraceInfo()
        {
            ViewData["TraceIdent"] = HttpContext.TraceIdentifier;
            ViewData["NumLogs"] = HttpRequestLog.GetHttpRequestLog(HttpContext.TraceIdentifier).RequestLogs.Count;
        }
    }
}