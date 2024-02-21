﻿using BeltmanSoftwareDesign.Business.Interfaces;
using BeltmanSoftwareDesign.Business.Models;
using BeltmanSoftwareDesign.Data;
using BeltmanSoftwareDesign.Data.Factories;
using BeltmanSoftwareDesign.Shared.Attributes;
using BeltmanSoftwareDesign.Shared.RequestJsons;
using BeltmanSoftwareDesign.Shared.ResponseJsons;
using BeltmanSoftwareDesign.StorageBlob.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeltmanSoftwareDesign.Business.Services
{
    public class RateService : IRateService
    {
        ApplicationDbContext db { get; }
        IStorageFileService StorageFileService { get; }
        IAuthenticationService AuthenticationService { get; }
        RateFactory RateFactory { get; }

        public RateService(
            ApplicationDbContext db,
            IStorageFileService storageFileService,
            IAuthenticationService authenticationService)
        {
            this.db = db;
            StorageFileService = storageFileService;
            AuthenticationService = authenticationService;
            RateFactory = new RateFactory(storageFileService);
        }

        [TsServiceMethod("Rate", "Create")]
        public RateCreateResponse Create(RateCreateRequest request, string? ipAddress, KeyValuePair<string, string?>[]? headers)
        {
            var authentication = AuthenticationService.GetState(
                request.BearerId, request.CurrentCompanyId, ipAddress, headers);
            if (!authentication.Success)
                return new RateCreateResponse()
                {
                    ErrorAuthentication = true
                };

            if (authentication.DbCurrentCompany == null)
                throw new Exception("Current company not chosen or doesn't exist, please create a company or select one.");

            var dbrate = RateFactory.Create(request.Rate, authentication.DbCurrentCompany, db);
            db.Rates.Add(dbrate);
            db.SaveChanges();

            return new RateCreateResponse()
            {
                Success = true,
                State = authentication,
                Rate = RateFactory.Create(dbrate)
            };
        }

        [TsServiceMethod("Rate", "Read")]
        public RateReadResponse Read(RateReadRequest request, string? ipAddress, KeyValuePair<string, string?>[]? headers)
        {
            var authentication = AuthenticationService.GetState(
                request.BearerId, request.CurrentCompanyId, ipAddress, headers);
            if (!authentication.Success)
                return new RateReadResponse()
                {
                    ErrorAuthentication = true
                };

            if (authentication.DbCurrentCompany == null)
                throw new Exception("Current company not chosen or doesn't exist, please create a company or select one.");

            var dbrate = db.Rates
                .Include(a => a.Company)
                .Include(a => a.TaxRate)
                .FirstOrDefault(a =>
                    a.CompanyId == authentication.DbCurrentCompany.id && 
                    a.id == request.RateId);
            if (dbrate == null)
                return new RateReadResponse()
                {
                    ErrorItemNotFound = true,
                    State = authentication
                };

            return new RateReadResponse()
            {
                Success = true,
                State = authentication,
                Rate = RateFactory.Create(dbrate)
            };
        }

        [TsServiceMethod("Rate", "Update")]
        public RateUpdateResponse Update(RateUpdateRequest request, string? ipAddress, KeyValuePair<string, string?>[]? headers)
        {
            var authentication = AuthenticationService.GetState(
                request.BearerId, request.CurrentCompanyId, ipAddress, headers);
            if (!authentication.Success)
                return new RateUpdateResponse()
                {
                    ErrorAuthentication = true
                };

            if (authentication.DbCurrentCompany == null)
                throw new Exception("Current company not chosen or doesn't exist, please create a company or select one.");

            var dbrate = db.Rates
                .Include(a => a.Company)
                .Include(a => a.TaxRate)
                .FirstOrDefault(a =>
                    a.CompanyId == authentication.DbCurrentCompany.id &&
                    a.id == request.Rate.id);
            if (dbrate == null)
                return new RateUpdateResponse()
                {
                    ErrorItemNotFound = true,
                    State = authentication
                };

            if (RateFactory.Copy(request.Rate, dbrate, authentication.DbCurrentCompany, db))
                db.SaveChanges();

            return new RateUpdateResponse()
            {
                Success = true,
                State = authentication,
                Rate = RateFactory.Create(dbrate)
            };
        }

        [TsServiceMethod("Rate", "Delete")]
        public RateDeleteResponse Delete(RateDeleteRequest request, string? ipAddress, KeyValuePair<string, string?>[]? headers)
        {
            var authentication = AuthenticationService.GetState(
                request.BearerId, request.CurrentCompanyId, ipAddress, headers);
            if (!authentication.Success)
                return new RateDeleteResponse()
                {
                    ErrorAuthentication = true
                };

            if (authentication.DbCurrentCompany == null)
                throw new Exception("Current company not chosen or doesn't exist, please create a company or select one.");

            var dbrate = db.Rates
                .Include(a => a.Company)
                .Include(a => a.TaxRate)
                .FirstOrDefault(a =>
                    a.CompanyId == authentication.DbCurrentCompany.id && 
                    a.id == request.RateId);
            if (dbrate == null)
                return new RateDeleteResponse()
                {
                    ErrorItemNotFound = true,
                    State = authentication
                };

            db.Rates.Remove(dbrate);
            db.SaveChanges();

            return new RateDeleteResponse()
            {
                Success = true,
                State = authentication
            };
        }

        [TsServiceMethod("Rate", "List")]
        public RateListResponse List(RateListRequest request, string? ipAddress, KeyValuePair<string, string?>[]? headers)
        {
            var authentication = AuthenticationService.GetState(
                request.BearerId, request.CurrentCompanyId, ipAddress, headers);
            if (!authentication.Success)
                return new RateListResponse()
                {
                    ErrorAuthentication = true
                };

            if (authentication.DbCurrentCompany == null)
                throw new Exception("Current company not chosen or doesn't exist, please create a company or select one.");

            var list = db.Rates
                .Include(a => a.Company)
                .Include(a => a.TaxRate)
                .Where(a => a.CompanyId == authentication.DbCurrentCompany.id)
                .Select(a => RateFactory.Create(a))
                .ToArray();

            return new RateListResponse()
            {
                Success = true,
                State = authentication,
                Rates = list
            };
        }
    }
}