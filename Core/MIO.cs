﻿namespace Mio;

public static class MIOWorkflow
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
// fiberid
public interface MIO<A>
{
    MIO<B> As<B>(B b) => 
        this.Map((_) => b);

    MIO<A> CatchAll(Func<Exception, MIO<A>> f) =>
            FoldMIO(e => f(e), a => MIO.SucceedNow(a));

    MIO<A> Ensuring(MIO<Unit> finalizer) =>
        FoldCauseMIO(
            cause => finalizer.ZipRight(() => MIO.Failure<A>(() => cause)),
            a => finalizer.ZipRight(() => MIO.SucceedNow(a)));

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
                CauseTags.Die => MIO.Failure<B>(() => cause),
                _ => throw new Exception("Impossible")
            };
        }, success);

    MIO<B> FoldCauseMIO<B>(Func<Cause, MIO<B>> failure, Func<A, MIO<B>> success) =>
        new Fold<A, B>(this, failure, success);

    MIO<Unit> Forever()
    {
        return this.ZipRight(() => Forever());
    }

    MIO<Fiber<A>> Fork() =>
        new Fork<A>(this);

    MIO<A> Interruptible() =>
        SetInterruptStatus(InterruptStatus.Interruptible);

    MIO<B> Map<B>(Func<A, B> f) =>
        this.FlatMap((a) => MIO.SucceedNow(f(a)));

    MIO<Unit> Repeat(int n)
    {
        if (n <= 0) return MIO.SucceedNow(Unit());
        else return this.ZipRight(() => Repeat(n - 1));
    }

    MIO<A> SetInterruptStatus(InterruptStatus status) =>
        new SetInterruptStatus<A>(this, status);

    MIO<A> Uninterruptible() =>
        SetInterruptStatus(InterruptStatus.Uninterruptible);

    internal sealed Fiber<A> UnsafeRunFiber() =>
        new FiberContext<A>(this, Executor.Default());

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

public enum Tags
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

public class Async<A> : MIO<A>
{
    public Tags Tag => Tags.Async;
    public Func<Func<A, Unit>, Unit> Register { get; }
    public Async(Func<Func<A, Unit>, Unit> register)
    {
        this.Register = register;
    }

    public Unit Resume(Func<MIO<A>, Unit> resume)
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        return this.Register(a =>
        {
            return resume(MIO.SucceedNow(a));
        });
    }

    public Unit Complete(Func<Exit<A>, Unit> complete) =>
        this.Register(a =>
        {
            return complete(Exit.Succeed<A>(a));
        });
}

public class Fail<A> : MIO<A>
{
    public Tags Tag => Tags.Fail;
    public Func<Cause> Cause { get; }
    public Fail(Func<Cause> cause)
    {
        this.Cause = cause;
    }
}

public class FlatMap<A, B> : MIO<B>
{
    public Tags Tag => Tags.FlatMap;
    public string Label
    {
        get => "FlatMap";
    }
    public MIO<A> Mio { get; }
    public Func<A, MIO<B>> Cont { get; }
    public FlatMap(MIO<A> mio, Func<A, MIO<B>> cont)
    {
        this.Mio = mio;
        this.Cont = cont;
    }
    public MIO<B> Apply(A value) => this.Cont(value);
}

public class Fold<A, B> : MIO<B>
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

public class Fork<A> : MIO<Fiber<A>>
{
    public Tags Tag => Tags.Fork;
    public MIO<A> Mio { get; }
    public Fork(MIO<A> mio)
    {
        this.Mio = mio;
    }
    internal FiberContext<A> CreateFiber(Executor executor) =>
        new FiberContext<A>(Mio, executor);
}

class SetInterruptStatus<A> : MIO<A>
{
    public Tags Tag => Tags.InterruptStatus;
    public MIO<A> MIO { get; }
    public InterruptStatus InterruptStatus { get; }
    public SetInterruptStatus(MIO<A> MIO, InterruptStatus interruptStatus)
    {
        this.MIO = MIO;
        this.InterruptStatus = interruptStatus;
    }
    public MIO<A> EnsuringOldStatus(MIO<Unit> finalizer) =>
        MIO.Ensuring(finalizer);
}

public class Succeed<A> : MIO<A>
{
    public Tags Tag => Tags.Succeed;
    public Func<A> Effect { get; }
    public Succeed(Func<A> effect)
    {
        this.Effect = effect;
    }
}

public class SucceedNow<A> : MIO<A>
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
    public static MIO<A> Async<A>(Func<Func<A, Unit>, Unit> register) =>
        new Async<A>(register);
    public static MIO<Unit> Die(Exception ex) =>
        Failure<Unit>(() => Cause.Die(ex));
    public static MIO<A> Done<A>(Exit<A> Exit) =>
        Exit.Match(ex => Failure<A>(() => ex), a => SucceedNow(a));
    public static MIO<A> Fail<A>(Func<Exception> ex) =>
        new Fail<A>(() => Cause.Fail(ex()));
    public static MIO<A> Failure<A>(Func<Cause> cause) =>
        new Fail<A>(cause);
    public static MIO<A> Succeed<A>(Func<A> f) =>
        new Succeed<A>(f);
    internal static MIO<A> SucceedNow<A>(A value) =>
        new SucceedNow<A>(value);
}
