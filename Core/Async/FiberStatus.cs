namespace Mio;
internal enum FiberStatusTags
{
    Done,
    Finishing,
    Running,
    Suspended
}

public interface FiberStatus
{
    internal FiberStatusTags Tag { get; }
    public static FiberStatus Done() => 
        new Done();
    public static FiberStatus Finishing(bool interrupting) => 
        new Finishing(interrupting);
    public static FiberStatus Running(bool interrupting) => 
        new Running(interrupting);
    public static FiberStatus Suspended(
        FiberStatus previous,
        bool interruptible,
        int epoch) 
        => new Suspended(previous, interruptible, epoch);
}

public class Done : FiberStatus 
{
    FiberStatusTags FiberStatus.Tag => FiberStatusTags.Done;
    internal Done() {}
}

public class Running : FiberStatus 
{
    FiberStatusTags FiberStatus.Tag => FiberStatusTags.Running;
    public bool Interrupting { get; }
    internal Running(bool interrupting)
    {
        this.Interrupting = interrupting;
    }
}

public class Finishing : FiberStatus 
{
    FiberStatusTags FiberStatus.Tag => FiberStatusTags.Finishing;
    public bool Interrupting { get; }
    internal Finishing(bool interrupting)
    {
        this.Interrupting = interrupting;
    }
}

public class Suspended : FiberStatus 
{
    FiberStatusTags FiberStatus.Tag => FiberStatusTags.Suspended;
    public FiberStatus Previous { get; }
    public bool Interruptible { get; }
    public int Epoch { get; }
    internal Suspended(FiberStatus previous,
        bool interruptible,
        int epoch)
    {
        this.Previous = previous;
        this.Interruptible = interruptible;
        this.Epoch = epoch;
    }
}