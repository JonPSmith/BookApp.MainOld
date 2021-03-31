// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModMon.Books.ServiceLayer.Common;
using ModMon.Books.ServiceLayer.Common.Dtos;
using ModMon.Books.ServiceLayer.GoodLinq;
using ModMon.Books.ServiceLayer.Udfs;
using ModMon.Books.ServiceLayer.Udfs.Dtos;
using ModMon.LoggingServices;

namespace ModMon.UI.Controllers
{
    public class UdfsSqlController : BaseTraceController
    {
        public async Task<IActionResult> Index(SortFilterPageOptions options, [FromServices] IListUdfsBooksService service)
        {
            var bookList = await (await service.SortFilterPageAsync(options))
                .ToListAsync();

            SetupTraceInfo();

            return View(new UdfsBookListCombinedDto(options, bookList));
        }

        /// <summary>
        /// This provides the filter search dropdown content
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetFilterSearchContent(SortFilterPageOptions options, [FromServices] IBookFilterDropdownService service)
        {
            var traceIdent = HttpContext.TraceIdentifier;
            return Json(
                new TraceIndentGeneric<IEnumerable<DropdownTuple>>(
                    traceIdent,
                    service.GetFilterDropDownValues(
                        options.FilterBy)));
        }
    }
}