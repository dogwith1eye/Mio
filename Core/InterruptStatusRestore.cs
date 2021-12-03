namespace Mio;

public class InterruptStatusRestore
{
    private InterruptStatus Flag { get; }
    public InterruptStatusRestore(InterruptStatus flag)
    {
        this.Flag = flag;
    }

    public MIO<A> Apply<A>(Func<MIO<A>> mio) => 
        mio().SetInterruptStatus(Flag);
}

public static class InterruptStatusRestoreHelper
{
    public static  InterruptStatusRestore restoreInterruptible = 
        new InterruptStatusRestore(InterruptStatus.Interruptible);
    
    public static  InterruptStatusRestore restoreUninterruptible = 
        new InterruptStatusRestore(InterruptStatus.Uninterruptible);

    public static InterruptStatusRestore Apply(InterruptStatus flag)
    {
        if (flag == InterruptStatus.Interruptible)
        {
            return restoreInterruptible;
        }
        else
        {
            return restoreUninterruptible;
        }
    }
}