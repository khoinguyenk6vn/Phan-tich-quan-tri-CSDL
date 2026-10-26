using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FashionFlow.DAL;

public sealed class FashionFlowContextFactory : IDesignTimeDbContextFactory<FashionFlowDbContext>
{
    public FashionFlowDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<FashionFlowDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new FashionFlowDbContext(options);
    }

    public FashionFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("FASHIONFLOW_CONNECTION_STRING")
            ?? "Server=.\\SQLEXPRESS;Database=FashionFlowDb;Trusted_Connection=True;TrustServerCertificate=True;";
        return Create(connectionString);
    }
}
