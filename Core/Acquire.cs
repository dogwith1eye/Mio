namespace Mio;

public class Acquire<A>
{
    public Func<MIO<A>> Effect { get; }
    public Acquire(Func<MIO<A>> effect)
    {
        this.Effect = effect;
    }
    public Release<A> Apply(Func<A, MIO<Unit>> release) =>
        new Release<A>(Effect, release);
}

public class Release<A>
{
    public Func<MIO<A>> Acquire { get; }
    public Func<A, MIO<Unit>> Effect { get; }
    public Release(Func<MIO<A>> acquire, Func<A, MIO<Unit>> effect)
    {
        this.Acquire = acquire;
        this.Effect = effect;
    }
    public MIO<B> Apply<B>(Func<A, MIO<B>> use) =>
        MIO.AcquireReleaseWith(Acquire, Effect, use);
}