using Mio.Middleware;

namespace Mio.Data;

public class ConnectionString
{
    string Value { get; }
    public ConnectionString(string value) { Value = value; }
    public static implicit operator string(ConnectionString c) => c.Value;
    public static implicit operator ConnectionString(string s) => new ConnectionString(s);
    public override string ToString() => Value;
}

public static class ConnectionStringMIO
{
    public static MIO<IEnumerable<A>> Query<A>(this ConnectionString connectionString, SqlTemplate sql)
            => Open(connectionString, conn => MIO.Succeed(() => conn.Query<A>(sql)));

    public static MIO<B> Open<B>(this ConnectionString connectionString, Func<SqlConnection, MIO<B>> use) =>
        MIO.AcquireReleaseWith(() => MIO.Succeed(() => new SqlConnection(connectionString)))
            .Apply(conn => MIO.Succeed(() => conn.Close()))
            .Apply(conn => use(conn));

    public static MIO<B> Begin<B>(this SqlConnection connection, Func<SqlTransaction, MIO<B>> use) =>
        MIO.AcquireReleaseWith(() => MIO.Succeed(() => connection.BeginTransaction()))
            .Apply(tran => MIO.Succeed(() => tran.Commit()))
            .Apply(tran => use(tran));
}

public static class ConnectionStringMiddleware
{
    public static Middleware<SqlConnection> Connect(ConnectionString connString) =>
        from conn in Using(new SqlConnection(connString))
        from _ in Succeed(() => conn.Open())
        select conn;

    public static Middleware<SqlTransaction> Transact(SqlConnection conn) =>
            from tran in Using(conn.BeginTransaction())
            from _ in SucceedAfter(tran, () => tran.Commit())
            select tran;

    public static Middleware<Unit> Succeed(Action f) =>
        cont => 
        {
            f();
            return cont(Unit());
        };

    public static Middleware<A> Succeed<A>(Func<A> f) =>
        cont => cont(f());

    public static Middleware<A> SucceedAfter<A>(A a, Action f) =>
        cont => 
        {
            var b = cont(a);
            f();
            return b;
        };
    
    public static Middleware<A> Using<A>(A disposable) where A : IDisposable =>
        cont =>
        {
            using (disposable) return cont(disposable);
        };
}