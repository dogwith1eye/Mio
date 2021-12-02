namespace Mio.Internal;

public struct AtomicBoolean
{
    private long _value;
    public bool Value 
    {
        get
        {
            return Interlocked.Read(ref _value) == 1;
        }
        set
        {
            Interlocked.Exchange(ref _value, Convert.ToInt64(value));
        }
    }
    public AtomicBoolean(bool value)
    {
        this._value = value ? 1 : 0;
    }
} 