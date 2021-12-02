using Unit = System.ValueTuple;

namespace Mio
{
    public interface Fiber<A>
    {
        MIO<A> Join();
        MIO<Unit> Interrupt();
    }
}