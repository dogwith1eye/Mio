namespace Mio;

public struct Exit<A>
{
    internal Cause Exception { get; }
    internal A Value { get; }
    
    public bool Success => Exception is null;
    public bool Failure => Exception is not null;

    internal Exit(Cause exception)
    {
        Exception = exception;
        Value = default(A);
    }

    internal Exit(A value)
    {
        Value = value;
        Exception = null;
    }

    public Z Fold<Z>(Func<Cause, Z> failed, Func<A, Z> completed) =>
        this.Match(
            cause => failed(cause),
            v => completed(v)
        );

    public B Match<B>(Func<Cause, B> Cause, Func<A, B> Success)
        => this.Failure ? Cause(Exception) : Success(Value);

    public override string ToString() 
        => Match(
            ex => $"{ex.ThreadId} Failure({ex.Tag}({ex.Exception.Message} {ex.Exception.StackTrace}))",
            t => $"Success({t})");
}

static class Exit
{
    public static Exit<A> Default<A>() =>
        new Exit<A>(default(A));
    public static Exit<A> Fail<A>(Exception ex) =>
        Exit.Failure<A>(Cause.Fail(ex));
    public static Exit<A> Failure<A>(Cause cause) => 
        new Exit<A>(cause);
    public static Exit<A> Succeed<A>(A value) => 
        new Exit<A>(value);
}
