﻿using Cafe.Core.AuthContext;
using Cafe.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Optional;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace Cafe.Api.Controllers
{
    [Route("api/[controller]")]
    public class ApiController : Controller
    {
        public Guid CurrentUserId
        {
            get
            {
                var idClaim = User?
                    .Claims?
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?
                    .Value;

                return string.IsNullOrEmpty(idClaim) ?
                    Guid.Empty :
                    Guid.Parse(idClaim);
            }
        }

        public Option<Guid> BaristaId => TryGetGuidClaim(AuthConstants.ClaimTypes.BaristaId);

        /// <summary>
        /// Enables using method groups when matching on Unit.
        /// </summary>
        protected IActionResult Ok(Unit unit) =>
            // Returning an empty object instead of an empty response to avoid invalid JSON errors
            Ok(new { });

        protected IActionResult NotFound(Error error) =>
            NotFound((object)error);

        protected IActionResult Error(Error error)
        {
            switch (error.Type)
            {
                case ErrorType.Validation:
                    return BadRequest(error);
                case ErrorType.NotFound:
                    return NotFound(error);
                case ErrorType.Unauthorized:
                    return Unauthorized(error);
                case ErrorType.Conflict:
                    return Conflict(error);
                case ErrorType.Critical:
                    // This shouldn't really happen as critical errors are there to be used by the generic exception filter
                    return new ObjectResult(error)
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                default:
                    return BadRequest(error);
            }
        }

        private Option<Guid> TryGetGuidClaim(string claimType)
        {
            var claimValue = User
                .Claims
                .FirstOrDefault(c => c.Type == claimType)?
                .Value;

            return claimValue
                .SomeNotNull()
                .Filter(v => Guid.TryParse(v, out Guid _))
                .Map(v => new Guid(v));
        }
    }
}
