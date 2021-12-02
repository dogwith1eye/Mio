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
    public int ThreadId { get; }
    public static Cause Die(Exception exception) => 
        new Die(exception);
    public static Cause Fail(Exception exception) => 
        new Fail(exception);
    public static Cause Interrupt(Exception exception) => 
        new Interrupt(exception);
}

public class Die : Cause 
{
    CauseTags Cause.Tag => CauseTags.Die;
    public Exception Exception => _exception;
    private Exception _exception;
    public int ThreadId => _threadId;
    private int _threadId;
    internal Die(Exception exception)
    {
        this._exception = exception;
        this._threadId = Thread.CurrentThread.ManagedThreadId;
    }
}

public class Fail : Cause 
{
    CauseTags Cause.Tag => CauseTags.Fail;
    public Exception Exception => _exception;
    private Exception _exception;
    public int ThreadId => _threadId;
    private int _threadId;
    internal Fail(Exception exception)
    {
        this._exception = exception;
        this._threadId = Thread.CurrentThread.ManagedThreadId;
    }
}

public class Interrupt : Cause 
{
    CauseTags Cause.Tag => CauseTags.Interrupt;
    public Exception Exception => _exception;
    private Exception _exception;
    public int ThreadId => _threadId;
    private int _threadId;    
    internal Interrupt(Exception exception)
    {
        this._exception = exception;
        this._threadId = Thread.CurrentThread.ManagedThreadId;
    }
}