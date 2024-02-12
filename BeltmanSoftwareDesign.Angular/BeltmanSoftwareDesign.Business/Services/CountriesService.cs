﻿using BeltmanSoftwareDesign.Business.Interfaces;
using BeltmanSoftwareDesign.Data;
using BeltmanSoftwareDesign.Data.Factories;
using BeltmanSoftwareDesign.Shared.Attributes;
using BeltmanSoftwareDesign.Shared.RequestJsons;
using BeltmanSoftwareDesign.Shared.ResponseJsons;

namespace BeltmanSoftwareDesign.Business.Services
{
    public class CountriesService : ICountriesService
    {
        ApplicationDbContext db { get; }
        IAuthenticationService AuthenticationService { get; }
        CountryFactory CountryFactory { get; }

        public CountriesService(
            ApplicationDbContext db,
            IAuthenticationService authenticationService)
        {
            this.db = db;
            AuthenticationService = authenticationService;
            CountryFactory = new CountryFactory();
        }

        [TsServiceMethod("Countries", "List")]
        public CountryListResponse List(CountryListRequest request, string? ipAddress, KeyValuePair<string, string?>[]? headers)
        {
            if (ipAddress == null)
                return new CountryListResponse()
                {
                    ErrorAuthentication = true
                };

            var authentication = AuthenticationService.GetState(
                request.BearerId, request.CurrentCompanyId, ipAddress, headers);
            if (!authentication.Success)
                return new CountryListResponse()
                {
                    ErrorAuthentication = true
                };

            var list = db.Countries
                .Select(a => CountryFactory.Convert(a))
                .ToArray();

            return new CountryListResponse()
            {
                Success = true,
                State = authentication,
                Countries = list
            };
        }
    }
}