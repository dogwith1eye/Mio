namespace Mio.Async;
internal enum CancelerStateTags
{
    Empty,
    Pending,
    Registered
}
internal interface CancelerState
{
    internal CancelerStateTags Tag { get; }
    public static CancelerState Empty() =>
        new Empty();
    public static CancelerState Pending() =>
        new Pending();
    public static CancelerState Registered(dynamic asyncCanceler) =>
        new Registered(asyncCanceler);
}

internal class Empty : CancelerState
{
    CancelerStateTags CancelerState.Tag => CancelerStateTags.Empty;
    public Empty() { }
}

internal class Pending : CancelerState
{
    CancelerStateTags CancelerState.Tag => CancelerStateTags.Pending;
    public Pending() { }
}

internal class Registered : CancelerState
{
    CancelerStateTags CancelerState.Tag => CancelerStateTags.Registered;
    public dynamic AsyncCanceler { get; }
    public Registered(dynamic asyncCanceler)
    {
        this.AsyncCanceler = asyncCanceler;
    }
}