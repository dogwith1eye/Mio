namespace Mio;

public struct Exit<A>
{
    internal Cause? Exception { get; }
    internal A? Value { get; }
    
    public bool Success => Exception is null;
    public bool Failure => Exception is not null;

    internal Exit(Cause? exception)
    {
        Exception = exception;
        Value = default(A);
    }

    internal Exit(A? value)
    {
        Value = value;
        Exception = null;
    }

    public B Match<B>(Func<Cause, B> Cause, Func<A, B> Success)
        => this.Failure 
        ? Cause((Exception is null) ? throw new ArgumentNullException(nameof(Exception)) : Exception) 
        : Success((Value is null) ? throw new ArgumentNullException(nameof(Value)) : Value);

    public override string ToString() 
        => Match(
            ex => $"Failure({ex.Tag}({ex.Exception.Message}))",
            t => $"Success({t})");
}

static class EXIT
{
    public static Exit<A> Default<A>() =>
        new Exit<A>(default(A));
    public static Exit<A> Fail<A>(Exception ex) =>
        EXIT.Failure<A>(CAUSE.Fail(ex));
    public static Exit<A> Failure<A>(Cause cause) => 
        new Exit<A>(cause);
    public static Exit<A> Succeed<A>(A value) => 
        new Exit<A>(value);
}
