using System.Runtime.CompilerServices;
[assembly: InternalsVisibleToAttribute("Core")]

var app = MyApp.Create();
app.Main(args);

public class Order
{
    public string Id { get; set; }
}

class MyApp : MIOApp<IEnumerable<Order>>
{
    public MIO<IEnumerable<Order>> Run()
    {
        ConnectionString connectionString = "Server=localhost;Database=SALES;Integrated Security=true;";
        SqlTemplate sqlOrders = "select * from orders";
        return connectionString.Query<Order>(sqlOrders);
    }
    public static MIOApp<IEnumerable<Order>> Create() => new MyApp();
}