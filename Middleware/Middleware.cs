namespace Mio.Middleware;

public delegate dynamic Middleware<A>(Func<A, dynamic> cont);

public static class MiddlewareWorkflow
{
    public static A Run<A>(this Middleware<A> mw) => mw(a => a);

    public static Middleware<B> Select<A, B>
        (this Middleware<A> mw, Func<A, B> f)
       => cont => mw(t => cont(f(t)));

    public static Middleware<B> SelectMany<A, B>
        (this Middleware<A> mw, Func<A, Middleware<B>> f)
        => cont => mw(t => f(t)(cont));

    public static Middleware<BB> SelectMany<A, B, BB>
        (this Middleware<A> @this, Func<A, Middleware<B>> f, Func<A, B, BB> project)
        => cont => @this(t => f(t)(r => cont(project(t, r))));
}