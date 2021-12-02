using System;
using System.Collections.Immutable;
using Unit = System.ValueTuple;
using static Mio.Prelude;

namespace Mio.Internal
{
    internal enum FiberStateTags
    {
        Done,
        Executing
    }
    internal interface FiberState<A> 
    {
        internal FiberStateTags Tag { get; }
    }

    internal class Done<A> : FiberState<A>
    {
        FiberStateTags FiberState<A>.Tag => FiberStateTags.Done;
        public Exit<A> Result { get; }
        public Done(Exit<A> result)
        {
            this.Result = result;
        }
    }

    internal class Executing<A> : FiberState<A>
    {
        FiberStateTags FiberState<A>.Tag => FiberStateTags.Executing;
        public ImmutableList<Func<Exit<A>, Unit>> Callbacks { get; }
        public Executing(ImmutableList<Func<Exit<A>, Unit>> callbacks)
        {
            this.Callbacks = callbacks;
        }
    }

    static class FiberState
    {
        public static FiberState<A> Done<A>(Exit<A> result) => 
            new Done<A>(result);
        public static FiberState<A> Executing<A>(ImmutableList<Func<Exit<A>, Unit>> callbacks) => 
            new Executing<A>(callbacks);
        public static FiberState<A> Initial<A>() => 
            Executing<A>(ImmutableList<Func<Exit<A>, Unit>>.Empty);
    }  
}