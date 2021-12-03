namespace Mio;

public enum InterruptStatus { Interruptible, Uninterruptible }

static class InterruptStatusEnum
{
    public static InterruptStatus FromBoolean(bool status) => status switch
    {
        
        true => InterruptStatus.Interruptible,
        false => InterruptStatus.Uninterruptible
    };

    public static bool ToBoolean(InterruptStatus status) => status switch
    {
        
        InterruptStatus.Interruptible => true,
        InterruptStatus.Uninterruptible => false,
        _ => throw new Exception("Impossible")
    };
}