namespace Mio;

public interface Fiber<A>
{
    MIO<A> Join();
    MIO<Unit> Interrupt();
}