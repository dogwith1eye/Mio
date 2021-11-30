using Mio.Internal;

namespace Mio;

static class MIOWorkflow
{
    public static MIO<B> Select<A, B>
        (this MIO<A> self, Func<A, B> f)
        => self.Map(f);

    public static MIO<B> SelectMany<A, B>
        (this MIO<A> self, Func<A, MIO<B>> f)
        => self.FlatMap(f);

    public static MIO<BB> SelectMany<A, B, BB>
        (this MIO<A> self, Func<A, MIO<B>> f, Func<A, B, BB> project)
        => self.FlatMap(a => f(a).Map(b => project(a, b)));
}

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
// next effect
// unsafeTryDone close fiber scope
// unwind stack discardedFolds
// unwind stack finalizer
// fail unsafenext
public interface MIO<A>
{
    SynchronizationContext DefaultExecutor { get => new SynchronizationContext(); }

    MIO<A> CatchAll(Func<Exception, MIO<A>> f) =>
            FoldMIO(e => f(e), a => MIO.SucceedNow(a));

    MIO<B> FlatMap<B>(Func<A, MIO<B>> f) => 
        new FlatMap<A, B>(this, f);

    MIO<B> Fold<B>(Func<Exception, B> failure, Func<A, B> success) =>
        FoldMIO(e => MIO.SucceedNow(failure(e)), a => MIO.SucceedNow(success(a)));

    MIO<B> FoldMIO<B>(Func<Exception, MIO<B>> failure, Func<A, MIO<B>> success) =>
        FoldCauseMIO(cause =>
        {
            return cause.Tag switch
            {
                CauseTags.Fail => failure(cause.Exception),
                CauseTags.Die  => MIO.Failure<B>(() => cause),
                _ => throw new Exception("Impossible")
            };
        }, success);

    MIO<B> FoldCauseMIO<B>(Func<Cause, MIO<B>> failure, Func<A, MIO<B>> success) =>
            new Fold<A, B>(this, failure, success);

    MIO<B> Map<B>(Func<A, B> f) => 
        this.FlatMap((a) => MIO.SucceedNow(f(a)));

    MIO<Unit> Repeat(int n)
    {
        if (n <= 0) return MIO.SucceedNow(Unit());
        else return this.ZipRight(() => Repeat(n - 1));
    }

    internal Tags Tag { get; }

    internal sealed Fiber<A> UnsafeRunFiber() => 
        new FiberContext<A>(this, this.DefaultExecutor); 

    Exit<A> UnsafeRunSync()
    {
        var latch = new CountdownEvent(1);
        var result = Exit.Default<A>();
        var mio = this.FoldCauseMIO(
            cause =>
            {
                return MIO.Succeed(() =>
                {
                    result = Exit.Failure<A>(cause);
                    latch.Signal();
                    return Unit();
                });
            },
            a => 
            {
                return MIO.Succeed(() =>
                {
                    result = Exit.Succeed(a);
                    latch.Signal();
                    return Unit();
                });
            });
        mio.UnsafeRunFiber();
        latch.Wait();
        return result;
    }

    MIO<(A, B)> Zip<B>(Func<MIO<B>> that) => 
        ZipWith(that, (a, b) => (a, b));

    MIO<B> ZipRight<B>(Func<MIO<B>> that) => 
        ZipWith(that, (a, b) => b);

    MIO<C> ZipWith<B, C>(Func<MIO<B>> that, Func<A, B, C> f) => 
        from a in this
        from b in that()
        select f(a, b);
}

internal enum Tags
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

class Fail<A> : MIO<A>
{
    public Tags Tag => Tags.Fail;
    public Func<Cause> Cause { get; }
    public Fail(Func<Cause> cause)
    {
        this.Cause = cause;
    }
}

internal class FlatMap<A, B> : MIO<B>
{
    public Tags Tag => Tags.FlatMap;
    public MIO<A> Mio { get; }
    public Func<A, MIO<B>> Cont { get; }
    public FlatMap(MIO<A> mio, Func<A, MIO<B>> cont)
    {
        this.Mio = mio;
        this.Cont = cont;
    }
    public MIO<B> Apply(A value) => this.Cont(value);
}

class Fold<A, B> : MIO<B>
{
    public Tags Tag => Tags.Fold;
    public MIO<A> Mio { get; }
    public Func<A, MIO<B>> Success { get; }
    public Func<Cause, MIO<B>> Failure { get; }
    public Fold(MIO<A> mio, Func<Cause, MIO<B>> failure, Func<A, MIO<B>> success)
    {
        this.Mio = mio;
        this.Failure = failure;
        this.Success = success;
    }

    public MIO<B> Apply(A value) => this.Success(value);
}

internal class Succeed<A> : MIO<A>
{
    public Tags Tag => Tags.Succeed;
    public Func<A> Effect { get; }
    public Succeed(Func<A> effect)
    {
        this.Effect = effect;
    }
}

internal class SucceedNow<A> : MIO<A>
{
    public Tags Tag => Tags.SucceedNow;
    public A Value { get; }
    public SucceedNow(A value)
    {
        this.Value = value;
    }
}

public static class MIO
{
     public static MIO<A> Fail<A>(Func<Exception> ex) =>
        new Fail<A>(() => Cause.Fail(ex()));
    public static MIO<A> Failure<A>(Func<Cause> cause) =>
        new Fail<A>(cause);
    public static MIO<Unit> Die(Exception ex) =>
        Failure<Unit>(() => Cause.Die(ex));
    public static MIO<A> Succeed<A>(Func<A> f) => 
            new Succeed<A>(f);
    internal static MIO<A> SucceedNow<A>(A value) => 
        new SucceedNow<A>(value);
}
