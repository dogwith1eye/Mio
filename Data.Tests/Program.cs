using System.Runtime.CompilerServices;
[assembly: InternalsVisibleToAttribute("Core")]

ConnectionString connectionString = "Server=localhost;Database=SALES;Integrated Security=true;";

var myProgram = (Order order) =>
    from conn in Connect(connectionString)
    from tran in Transact(conn)
    from id in OrderQuery.Insert(tran, order)
    select id;

var app = MyApp.Create();
app.Main(args);

public static class OrderQuery
{
    public static Middleware<int> Insert(SqlTransaction tran, Order order) =>
        Succeed(() => order.Id);
    
    public static MIO<int> InsertMIO(SqlTransaction tran, Order order) =>
        MIO.Succeed(() => order.Id);
}

public class Order
{
    public int Id { get; set; }
}

class MyApp : MIOApp<int>
{
    static ConnectionString connectionString = "Server=localhost;Database=SALES;Integrated Security=true;";
    static Order order = new Order()
    {
        Id = 1
    };
    static MIO<int> MyProgram = 
        ConnectionStringMIO.Open<int>(connectionString,
            conn => ConnectionStringMIO.Begin<int>(conn,
                tran => OrderQuery.InsertMIO(tran, order)));
        
    public MIO<int> Run() => MyProgram;
    public static MIOApp<int> Create() => new MyApp();
}