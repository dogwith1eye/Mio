namespace Mio;

public interface Runnable 
{
    public Unit Run();
}

public class Executor
{
    public Executor(SynchronizationContext context)
    {
        this.Context = context;
    }

    private SynchronizationContext Context { get; }

    bool UnsafeSubmitAndYield(Runnable runnable) =>
        UnsafeSubmit(runnable);

    public Unit UnsafeSubmitAndYieldOrThrow(Runnable runnable)
    {
        if (!UnsafeSubmitAndYield(runnable)) throw new NotSupportedException($"Unable to run ${runnable}");
        return Unit();   
    }

    public Unit UnsafeSubmitOrThrow(Runnable runnable)
    {
        if (!UnsafeSubmit(runnable)) throw new NotSupportedException($"Unable to run ${runnable}");
        return Unit();   
    }

    public bool UnsafeSubmit(Runnable runnable)
    {
        try 
        {
            this.Context.Post(_ => runnable.Run(), null);
            return true;
        } 
        catch (NotSupportedException) 
        {
            return false;
        }
    }

    public static Executor Default() =>
        new Executor(new SynchronizationContext());
}