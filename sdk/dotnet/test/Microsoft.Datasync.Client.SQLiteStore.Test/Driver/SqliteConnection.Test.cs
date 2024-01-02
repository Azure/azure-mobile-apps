using Microsoft.Datasync.Client.SQLiteStore.Driver;
using SQLitePCL;

namespace Microsoft.Datasync.Client.SQLiteStore.Test.Driver;

public class SqliteConnection_Tests
{
    [Fact]
    public void Ctor_ConnectionString_SetsMaxParametersPerQuery()
    {
        var sut = new SqliteConnection("file:memory?mode=memory");
        Assert.NotNull(sut);
        Assert.True(sut.MaxParametersPerQuery > 16);
        Assert.True(SqliteConnection.sqliteIsInitialized);
        Assert.True(sut.handleSqliteLifecycle);
    }

    [Fact]
    public void Ctor_ConnectionString_SecondCall_DoesNotReinitialize()
    {
        var sut = new SqliteConnection("file:memory?mode=memory");
        Assert.NotNull(sut);
        Assert.True(sut.MaxParametersPerQuery > 16);
        Assert.True(SqliteConnection.sqliteIsInitialized);
        Assert.True(sut.handleSqliteLifecycle);

        var sut2 = new SqliteConnection("file:memory?mode=memory");
        Assert.NotNull(sut2);
        Assert.True(sut2.MaxParametersPerQuery > 16);
        Assert.True(SqliteConnection.sqliteIsInitialized);
        Assert.True(sut2.handleSqliteLifecycle);
    }

    [Fact]
    public void Ctor_Connection_SetsMaxParametersPerQuery()
    {
        Batteries_V2.Init();
        int rc = raw.sqlite3_open("file:memory?mode=memory", out sqlite3 connection);
        Assert.Equal(raw.SQLITE_OK, rc);

        var sut = new SqliteConnection(connection);
        Assert.NotNull(sut);
        Assert.True(sut.MaxParametersPerQuery > 16);
        Assert.True(SqliteConnection.sqliteIsInitialized);
        Assert.False(sut.handleSqliteLifecycle);
    }
}
