﻿using Cafe.Api.Hateoas.Resources;
using Cafe.Api.Hateoas.Resources.Barista;
using Cafe.Core.AuthContext;
using Cafe.Core.BaristaContext.Commands;
using Cafe.Core.BaristaContext.Queries;
using Cafe.Domain.Views;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Async;
using System.Threading.Tasks;

namespace Cafe.Api.Controllers
{
    [Authorize(Policy = AuthConstants.Policies.IsAdminOrManager)]
    public class BaristaController : ApiController
    {
        public BaristaController(IResourceMapper resourceMapper, IMediator mediator)
            : base(resourceMapper, mediator)
        {
        }

        /// <summary>
        /// Hires a new barista.
        /// </summary>
        [HttpPost(Name = nameof(HireBarista))]
        public async Task<IActionResult> HireBarista([FromBody] HireBarista command) =>
            (await Mediator.Send(command)
                .MapAsync(ToEmptyResourceAsync<HireBaristaResource>))
                .Match(Ok, Error);

        /// <summary>
        /// Retrieves currently employed baristas.
        /// </summary>
        [HttpGet(Name = nameof(GetEmployedBaristas))]
        public Task<IActionResult> GetEmployedBaristas() =>
            ResourceContainerResult<BaristaView, BaristaResource, BaristaContainerResource>(new GetEmployedBaristas());
    }
}
