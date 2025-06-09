using Microsoft.EntityFrameworkCore;
using Order_Management.Infrastructure.Data;

namespace Application.UnitTests.Utilities;
public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
