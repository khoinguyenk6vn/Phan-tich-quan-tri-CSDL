using FashionFlow.DAL;
using Microsoft.EntityFrameworkCore;

namespace FashionFlow.BLL;

public sealed record LoginSession(string UserName, string ConnectionString, bool IsAdmin);

public sealed class LoginService(FashionFlowContextFactory contextFactory)
{
    public async Task<LoginSession> SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Vui lòng nhập tài khoản và mật khẩu SQL Server.");

        var connectionString = BuildConnectionString(userName.Trim(), password);
        await using var db = contextFactory.Create(connectionString);
        if (!await db.Database.CanConnectAsync(cancellationToken))
            throw new InvalidOperationException("Không kết nối được SQL Server. Kiểm tra .\\SQLEXPRESS, database, tài khoản và mật khẩu.");

        var isAdmin = await db.Database.SqlQueryRaw<int>(
            "SELECT CASE WHEN IS_ROLEMEMBER(N'FashionFlowAdmin') = 1 THEN 1 ELSE 0 END AS [Value]")
            .SingleAsync(cancellationToken) == 1;
        if (isAdmin)
            await DatabaseInitializer.InitializeAsync(db, cancellationToken);

        return new LoginSession(userName.Trim(), connectionString, isAdmin);
    }

    private static string BuildConnectionString(string userName, string password)
    {
        static string Quote(string value) => "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
        return $"Server=.\\SQLEXPRESS;Database=FashionFlowDb;User ID={Quote(userName)};Password={Quote(password)};Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;";
    }
}
