namespace Mio.Internal;
internal enum FiberStateTags
{
    Done,
    Running
}
internal interface FiberState<A> 
{
    internal FiberStateTags Tag { get; }
}

internal class Done<A> : FiberState<A>
{
    FiberStateTags FiberState<A>.Tag => FiberStateTags.Done;
    public Exit<A> Result { get; }
    public Done(Exit<A> result)
    {
        this.Result = result;
    }
}

internal class Running<A> : FiberState<A>
{
    FiberStateTags FiberState<A>.Tag => FiberStateTags.Done;
    public List<Func<Exit<A>, Unit>> Callbacks { get; }
    public Running(List<Func<Exit<A>, Unit>> callbacks)
    {
        this.Callbacks = callbacks;
    }
}

static class FiberState
{
    public static FiberState<A> Done<A>(Exit<A> result) => 
        new Done<A>(result);
    public static FiberState<A> Running<A>(List<Func<Exit<A>, Unit>> callbacks) => 
        new Running<A>(callbacks);
    public static FiberState<A> Initial<A>() => 
        FiberState.Running(new List<Func<Exit<A>, Unit>>());
}  