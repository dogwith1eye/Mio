using System;
using System.Threading;

namespace Mio
{
    public interface MIOApp<A> 
    {
        MIO<A> Run();

        void Main(string[] args)
        {
            var result = this.Run().UnsafeRunSync();
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} The result was {result}");
        }
    }
}