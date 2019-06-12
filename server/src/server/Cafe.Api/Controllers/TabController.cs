﻿using Cafe.Api.Hateoas.Resources;
using Cafe.Api.Hateoas.Resources.Tab;
using Cafe.Core.AuthContext;
using Cafe.Core.TabContext.Commands;
using Cafe.Core.TabContext.Queries;
using Cafe.Domain.Views;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Async;
using System;
using System.Threading.Tasks;

namespace Cafe.Api.Controllers
{
    [Authorize(Policy = AuthConstants.Policies.IsAdminOrWaiter)]
    public class TabController : ApiController
    {
        public TabController(IResourceMapper resourceMapper, IMediator mediator)
            : base(resourceMapper, mediator)
        {
        }

        /// <summary>
        /// Closes a tab.
        /// </summary>
        [HttpPut("close", Name = nameof(CloseTab))]
        public async Task<IActionResult> CloseTab([FromBody] CloseTab command) =>
            (await Mediator.Send(command)
                .MapAsync(_ => ToEmptyResourceAsync<CloseTabResource>(x => x.TabId = command.TabId)))
                .Match(Ok, Error);

        /// <summary>
        /// Retrieves all open tabs.
        /// </summary>
        [HttpGet(Name = nameof(GetAllOpenTabs))]
        public Task<IActionResult> GetAllOpenTabs() =>
            ResourceContainerResult<TabView, TabResource, TabsContainerResource>(new GetAllOpenTabs());

        /// <summary>
        /// Retrieves all closed tabs.
        /// </summary>
        [HttpGet("history", Name = nameof(GetTabHistory))]
        public Task<IActionResult> GetTabHistory() =>
            ResourceContainerResult<TabView, TabResource, TabsContainerResource>(new GetTabHistory());

        /// <summary>
        /// Retrieves a tab by id.
        /// </summary>
        [HttpGet("{id}", Name = nameof(GetTabView))]
        public async Task<IActionResult> GetTabView(Guid id) =>
            (await Mediator.Send(new GetTabView { Id = id })
                .MapAsync(ToResourceAsync<TabView, TabResource>))
                .Match(Ok, Error);

        /// <summary>
        /// Opens a new tab on a given table.
        /// </summary>
        [HttpPost("open", Name = nameof(OpenTab))]
        public async Task<IActionResult> OpenTab([FromBody] OpenTab command) =>
            (await Mediator.Send(command)
                .MapAsync(_ => ToEmptyResourceAsync<OpenTabResource>(x => x.Id = command.Id)))
                .Match(Ok, Error);

        /// <summary>
        /// Orders a list of menu items for a given tab.
        /// </summary>
        [HttpPut("order", Name = nameof(OrderMenuItems))]
        public async Task<IActionResult> OrderMenuItems([FromBody] OrderMenuItems command) =>
            (await Mediator.Send(command)
                .MapAsync(_ => ToEmptyResourceAsync<OrderMenuItemsResource>(x => x.TabId = command.TabId)))
                .Match(Ok, Error);

        /// <summary>
        /// Rejects a list of menu items for a given tab.
        /// </summary>
        [HttpPut("reject", Name = nameof(RejectMenuItems))]
        public async Task<IActionResult> RejectMenuItems([FromBody] RejectMenuItems command) =>
            (await Mediator.Send(command)
                .MapAsync(_ => ToEmptyResourceAsync<RejectMenuItemsResource>(x => x.TabId = command.TabId)))
                .Match(Ok, Error);

        /// <summary>
        /// Serves a list of menu items.
        /// </summary>
        [HttpPut("serve", Name = nameof(ServeMenuItems))]
        public async Task<IActionResult> ServeMenuItems([FromBody] ServeMenuItems command) =>
            (await Mediator.Send(command)
                .MapAsync(_ => ToEmptyResourceAsync<ServeMenuItemsResource>(x => x.TabId = command.TabId)))
                .Match(Ok, Error);
    }
}