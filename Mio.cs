namespace Mio;

// maximum operation count
// log runtime
// eliminate heap usage for happy path flatmap
// stack trace for happy path
// succeed with
// trace 
// logged / unsafelog
// suspend / suspend with
// async enter suspend state
// shift current executor
// yield
// fiber ref
// race
// supervise
// fork scope
// runtime config
// bracket

interface ZIO<A>
{
    Tags Tag { get; }
    
    enum Tags
    {
        FlatMap,
        Fold,
        Ensuring,
        SucceedNow,
        Fail,
        Succeed,
        Async,
        InterruptStatus,
        Fork,
        Shift,
        Provide,
        Access
    }
}
