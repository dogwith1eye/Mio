namespace Mio.Data;

public class ConnectionString
{
    string Value { get; }
    public ConnectionString(string value) { Value = value; }
    public static implicit operator string(ConnectionString c) => c.Value;
    public static implicit operator ConnectionString(string s) => new ConnectionString(s);
    public override string ToString() => Value;
}

public static class ConnectionStringExtensions
{
    public static MIO<IEnumerable<A>> Query<A>(this ConnectionString connectionString, SqlTemplate sql)
            => Open(connectionString, conn => MIO.Succeed(() => conn.Query<A>(sql)));

    public static MIO<B> Open<B>(this ConnectionString connectionString, Func<SqlConnection, MIO<B>> use) =>
        MIO.AcquireReleaseWith(() => MIO.Succeed(() => new SqlConnection(connectionString)))
            .Apply(conn => MIO.Succeed(() => conn.Close()))
            .Apply(a => use(a));
}