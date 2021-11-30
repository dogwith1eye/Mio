namespace Mio;

public interface MIOApp<A> 
{
    MIO<A> Run();

    void Main(string[] args)
    {
        var result = this.Run().UnsafeRunSync();
        Console.WriteLine("The result was ${result}");
    }
}