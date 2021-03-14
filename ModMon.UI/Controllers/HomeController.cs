// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModMon.Books.Persistence;
using MonMon.UI.HelperExtensions;
using MonMon.UI.Models;

namespace MonMon.UI.Controllers
{
    public class HomeController : BaseTraceController
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DatabaseCounts([FromServices] BookDbContext context)
        {
            return View(new DatabaseStatsDto(context));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Chapter15Setup()
        {
            return View();
        }

        public IActionResult Chapter16Setup()
        {
            return View();
        }

        public IActionResult About()
        {
            var isLocal = Request.IsLocal();
            return View(isLocal);
        }

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        public IActionResult Error()
        {
            return View(new ErrorViewModel
                { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}