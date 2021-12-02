namespace Mio.Internal;

internal class FiberContext<A> : Fiber<A>, Runnable
{
    public FiberContext(MIO<A> startMio, Executor startExecutor)
    {
        this.currentExecutor = startExecutor;
        UnsafeRunLater(startMio);
    }

    private Executor currentExecutor = null;
    public AtomicBoolean Interrupted = new AtomicBoolean(false); 
    public AtomicBoolean Interrupting = new AtomicBoolean(false);
    public AtomicBoolean Interruptible = new AtomicBoolean(true); 
    volatile private dynamic nextEffect = null;
    private FiberState<A> state =  FiberState.Initial<A>();
    private Stack<dynamic> stack = new Stack<dynamic>();

    public MIO<Unit> Interrupt() =>
        MIO.Succeed(() => 
        {
            Interrupted.Value = true;
            return Unit();
        });

    public MIO<A> Join()
    {
        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Join {stack.Count()}");
        return MIO.Async<Exit<A>>(callback => 
        {
            UnsafeAwait(callback);
            return Unit();
        }).FlatMap<A>(MIO.Done);
    }

    public Unit Run()
    {
        dynamic curMio = this.nextEffect;
        this.nextEffect = null;

        // Put the stack reference on the stack:
        var stack = this.stack;

        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Running {stack.Count()}");

        while (curMio is not null)
        {
            try
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {curMio} {stack.Count()}");
                switch (curMio.Tag)
                {
                    case Tags.Async:
                        if (stack.Count == 0)
                        {      
                            Func<Exit<A>, Unit> tryDone = (a) => UnsafeTryDone(a);
                            curMio.Complete(tryDone);
                            curMio = null;
                        }
                        else
                        {
                            Func<dynamic, Unit> resume = (next) => 
                            {
                                UnsafeRunLater(next);
                                return Unit();
                            };
                            curMio.Resume(resume);
                            curMio = null;
                        }
                        break;

                    case Tags.Fail:
                        var errorHandler = UnsafeUnwindStack();
                        if (errorHandler is null)
                        {
                            UnsafeTryDone(Exit.Failure(curMio.Cause()));
                        }
                        else
                        {
                            curMio = errorHandler.Failure(curMio.Cause());
                        }
                        break;

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
                                curMio = curMio.Mio;
                                break;
                        }
                        break;

                    case Tags.Fold:
                        stack.Push(curMio);
                        curMio = curMio.Mio;
                        break;

                    case Tags.Fork:
                        var fiber = curMio.CreateFiber(this.currentExecutor);
                        curMio = UnsafeNextEffect(fiber);
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
            catch (Exception ex)
            {
                curMio = MIO.Die(ex);
            }
        } 
        return Unit();
    }

    public Unit UnsafeAwait(Func<Exit<A>, Unit> callback)
    {
        var loop = true;
        while (loop)
        {
            var oldState = state;
            switch (oldState.Tag)
            {
                case FiberStateTags.Executing:
                    var executing = (Executing<A>)oldState;
                    var newCallbacks = executing.Callbacks.Add(callback);
                    var newState = FiberState.Executing<A>(newCallbacks);
                    loop = (state != oldState || Interlocked.CompareExchange(ref state, newState, oldState) != oldState);
                    break;
                case FiberStateTags.Done:
                    var done = (Done<A>)oldState;
                    callback(done.Result);
                    loop = false;
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
            //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} UnsafeNextEffect count={stack.Count}");
            return cont.Apply(previousSuccess);
        }
        else
        {
            return UnsafeTryDone(Exit.Succeed<A>(previousSuccess));
        }
    }

    Unit UnsafeRunLater(dynamic mio)
    {
        nextEffect = mio;
        if (stack.Count == 0)
        {
            this.currentExecutor.UnsafeSubmitAndYieldOrThrow(this);
        }
        else
        {
            this.currentExecutor.UnsafeSubmitOrThrow(this);
        }
        return Unit();
    }

    Unit UnsafeTryDone(Exit<A> result)
    {
        var loop = true;
        var toComplete = ImmutableList<Func<Exit<A>, Unit>>.Empty;
        while (loop)
        {
            var oldState = state;
            switch (oldState.Tag)
            {
                case FiberStateTags.Executing:
                    var executing = (Executing<A>)oldState;
                    toComplete = executing.Callbacks;
                    var newState = FiberState.Done<A>(result);
                    loop = (state != oldState || Interlocked.CompareExchange(ref state, newState, oldState) != oldState);
                    break;
                case FiberStateTags.Done:
                    throw new Exception("Fiber being completed multiple times");
            }
        }
        toComplete?.ForEach(callback => callback(result));
        return Unit();
    }

    dynamic UnsafeUnwindStack()
    {
        var loop = true;
        dynamic errorHandler = null;
        while (loop && stack.Count > 0)
        {
            var curMio = stack.Pop();
            switch (curMio.Tag)
            {
                case Tags.Fold:
                    errorHandler = curMio;
                    loop = false;
                    break;
            }     
        }
        return errorHandler;
    } 
}