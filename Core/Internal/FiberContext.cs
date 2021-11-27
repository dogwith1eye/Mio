namespace Mio.Internal;
internal class FiberContext<A> : Fiber<A>
{
    private SynchronizationContext? currentExecutor = null;
    volatile private dynamic? nextEffect = null;
    private FiberState<A> state =  FIBER_STATE.Initial<A>();
    private Stack<dynamic> stack = new Stack<dynamic>();
    public FiberContext(MIO<A> startMio, SynchronizationContext startExecutor)
    {
        this.nextEffect = startMio;
        this.currentExecutor = startExecutor;
        this.currentExecutor.Post(_ => this.Run(), null);
    }
    public Unit Run()
    {
        dynamic? curMio = this.nextEffect;
        this.nextEffect = null;

        // Put the stack reference on the stack:
        var stack = this.stack;

        while (curMio is not null)
        {
            switch (curMio.Tag)
            {
                case Tags.FlatMap:
                    switch (curMio.Mio.Tag)
                    {
                        case Tags.SucceedNow:
                            curMio = curMio.Apply(curMio.Mio.Value);
                            break;

                        case Tags.Succeed:
                            var svalue = curMio.Mio.Effect();
                            curMio = curMio.Apply(svalue);
                            break;

                        default:
                            stack.Push(curMio);
                            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} FlatMap count={stack.Count}");
                            curMio = curMio.Mio;
                            break;
                    }
                    break;

                case Tags.Fold:
                    stack.Push(curMio);
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Fold count={stack.Count}");
                    curMio = curMio.Mio;
                    break;

                case Tags.SucceedNow:
                    var value = curMio.Value;
                    curMio = UnsafeNextEffect(curMio.Value);
                    break;
                
                case Tags.Succeed:
                    curMio = UnsafeNextEffect(curMio.Effect());
                    break;
            }
        } 
        return Unit();
    }

    dynamic UnsafeNextEffect(dynamic previousSuccess)
    {
        if (stack.Count > 0)
        {
            var cont = stack.Pop();
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} UnsafeNextEffect count={stack.Count}");
            return cont.Apply(previousSuccess);
        }
        else
        {
            return UnsafeTryDone(EXIT.Succeed<A>(previousSuccess));
        }
    }

    dynamic UnsafeTryDone(Exit<A> result)
    {
        var loop = true;
        var toComplete = new List<Func<Exit<A>, Unit>>();
        while (loop)
        {
            var oldState = Volatile.Read(ref state);
            switch (state.Tag)
            {
                case FiberStateTags.Running:
                    var running = oldState as Running<A>;
                    toComplete = running?.Callbacks;
                    loop = (state != oldState || Interlocked.CompareExchange(ref state, new Done<A>(result), oldState) != oldState);
                    break;
                case FiberStateTags.Done:
                    throw new Exception("Fiber being completed multiple times");
            }
        }
        toComplete?.ForEach(callback => callback(result));
        
        return Unit();
    }
}