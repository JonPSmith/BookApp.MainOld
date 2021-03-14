﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericServices;
using GenericServices.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModMon.Books.Domain;
using ModMon.Books.Domain.SupportTypes;
using ModMon.Books.Infrastructure.CachedValues;
using ModMon.Books.Infrastructure.CachedValues.CheckFixCode;
using ModMon.Books.Persistence;
using ModMon.Books.ServiceLayer.GoodLinq.Dtos;
using ModMon.LoggingServices;
using ModMon.UI.HelperExtensions;
using SoftDeleteServices.Concrete;

namespace ModMon.UI.Controllers
{
    public class AdminController : BaseTraceController
    {
        public class BookUpdatedDto
        {
            public BookUpdatedDto(string message)
            {
                Message = message;
            }

            public string Message { get; }
        }

        public async Task<IActionResult> ChangePubDate(int id, [FromServices]ICrudServicesAsync<BookDbContext> service) 
        {
            Request.ThrowErrorIfNotLocal(); 
            var dto = await service.ReadSingleAsync<ChangePubDateDto>(id); 
            SetupTraceInfo();
            return View(dto); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePubDate(ChangePubDateDto dto, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            await service.UpdateAndSaveAsync(dto);
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        public async Task<IActionResult> AddPromotion(int id, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            Request.ThrowErrorIfNotLocal();
            var dto = await service.ReadSingleAsync<AddPromotionDto>(id);
            SetupTraceInfo();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPromotion(AddPromotionDto dto, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            Request.ThrowErrorIfNotLocal();
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            await service.UpdateAndSaveAsync(dto);
            SetupTraceInfo();
            if (!service.HasErrors)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        public async Task<IActionResult> RemovePromotion(int id, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            Request.ThrowErrorIfNotLocal();
            var dto = await service.ReadSingleAsync<RemovePromotionDto>(id);
            if (!service.IsValid)
            {
                service.CopyErrorsToModelState(ModelState, dto);
            }
            SetupTraceInfo();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePromotion(RemovePromotionDto dto, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            Request.ThrowErrorIfNotLocal();
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            await service.UpdateAndSaveAsync(dto, nameof(Book.RemovePromotion));
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }


        public async Task<IActionResult> AddBookReview(int id, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            Request.ThrowErrorIfNotLocal();
            var dto = await service.ReadSingleAsync<AddReviewDto>(id);
            if (!service.IsValid)
            {
                service.CopyErrorsToModelState(ModelState, dto);
            }
            SetupTraceInfo();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBookReview(AddReviewDto dto, [FromServices] ICrudServicesAsync<BookDbContext> service)
        {
            Request.ThrowErrorIfNotLocal();
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            await service.UpdateAndSaveAsync(dto);
            SetupTraceInfo();
            if (service.IsValid)
                return View("BookUpdated", service.Message);

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            return View(dto);
        }

        public async Task<IActionResult> SoftDelete(int id, [FromServices] SingleSoftDeleteServiceAsync<ISoftDelete> service)
        {
            Request.ThrowErrorIfNotLocal();
            var status = await service.SetSoftDeleteViaKeysAsync<Book>(id);
            SetupTraceInfo();

            return View("BookUpdated", status.IsValid ? status.Message : status.GetAllErrors());
        }

        public async Task<IActionResult> ListSoftDeleted([FromServices] SingleSoftDeleteServiceAsync<ISoftDelete> service)
        {
            Request.ThrowErrorIfNotLocal();

            var softDeletedBooks = await service.GetSoftDeletedEntries<Book>()
                .Select(x => new SimpleBookList{BookId = x.BookId, LastUpdatedUtc = x.LastUpdatedUtc, Title = x.Title})
                .ToListAsync();

            SetupTraceInfo();
            return View(softDeletedBooks);
        }

        public async Task<IActionResult> UnSoftDelete(int id, [FromServices] SingleSoftDeleteServiceAsync<ISoftDelete> service)
        {
            Request.ThrowErrorIfNotLocal();
            var status = await service.ResetSoftDeleteViaKeysAsync<Book>(id);
            SetupTraceInfo();
            return View("BookUpdated", status.IsValid ? status.Message : status.GetAllErrors());
        }

        //------------------------------------------------------------
        //Admin parts

        public IActionResult CacheCheckFix()
        {
            Request.ThrowErrorIfNotLocal();
            SetupTraceInfo();
            return View(new CheckFixInputDto{ FixBadCacheValues = true, LookingBack = new TimeSpan(0,1,00,0)});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CacheCheckFix(CheckFixInputDto dto, CancellationToken cancellationToken, 
            [FromServices]ICheckFixCacheValuesService service)
        {
            Request.ThrowErrorIfNotLocal();
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var scanFrom = DateTime.UtcNow.Subtract(dto.LookingBack);
            var notes = await service.RunCheckAsync(scanFrom, dto.FixBadCacheValues, cancellationToken );
            SetupTraceInfo();
            return View("CacheCheckFixResult", notes);
        }

        public IActionResult GetTimingLogs()
        {
            var timingLogs = HttpTimingLog.GetTimingStats(5);
            return View(timingLogs);
        }

    }
}