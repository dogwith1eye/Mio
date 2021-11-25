namespace Mio;

internal enum CauseTags
{
    Die,
    Fail,
    Interrupt
}

public interface Cause
{
    internal CauseTags Tag { get; }
    public Exception Exception { get; }
}

public class Die : Cause 
{
    CauseTags Cause.Tag => CauseTags.Die;
    public Exception Exception => _exception;
    Exception Cause.Exception => throw new NotImplementedException();

    private Exception _exception;
    internal Die(Exception exception)
    {
        this._exception = exception;
    }
}

public class Fail : Cause 
{
    CauseTags Cause.Tag => CauseTags.Fail;
    public Exception Exception => _exception;
    private Exception _exception;
    internal Fail(Exception exception)
    {
        this._exception = exception;
    }
}

public class Interrupt : Cause 
{
    CauseTags Cause.Tag => CauseTags.Interrupt;
    public Exception Exception => _exception;
    private Exception _exception;
    internal Interrupt(Exception exception)
    {
        this._exception = exception;
    }
}

static class CAUSE
{
    public static Cause Die(Exception exception) => 
        new Die(exception);
    public static Cause Fail(Exception exception) => 
        new Fail(exception);
    public static Cause Interrupt(Exception exception) => 
        new Interrupt(exception);
}