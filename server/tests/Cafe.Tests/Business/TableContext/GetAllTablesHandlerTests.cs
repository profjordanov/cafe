﻿using Cafe.Core.TableContext.Queries;
using Cafe.Domain.Entities;
using Cafe.Tests.Customizations;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cafe.Tests.Business.TableContext
{
    public class GetAllTablesHandlerTests : ResetDatabaseLifetime
    {
        private readonly AppFixture _fixture;

        public GetAllTablesHandlerTests()
        {
            _fixture = new AppFixture();
        }

        [Theory]
        [CustomizedAutoData]
        public async Task CanGetAllTables(GetAllTables query, Table[] tablesToAdd)
        {
            // Arrange
            await _fixture.ExecuteDbContextAsync(async dbContext =>
            {
                var waiters = tablesToAdd
                    .Select(t => t.Waiter);

                dbContext.Waiters.AddRange(waiters);
                dbContext.Tables.AddRange(tablesToAdd);

                await dbContext.SaveChangesAsync();
            });

            // Act
            var tables = await _fixture.SendAsync(query);

            // Assert
            (tables.Count == tablesToAdd.Length &&
             tables.All(t => tablesToAdd.Any(addedTable => t.Number == addedTable.Number &&
                                                           t.WaiterId == addedTable.WaiterId &&
                                                           t.WaiterShortName == addedTable.Waiter.ShortName)))
            .ShouldBeTrue();
        }

        [Theory]
        [CustomizedAutoData]
        public async Task CanGetAllWhenNoneAreExisting(GetAllTables query)
        {
            // Arrange
            // Purposefully skipping adding any tables

            // Act
            var tables = await _fixture.SendAsync(query);

            // Assert
            (tables.Count == 0).ShouldBeTrue();
        }
    }
}
