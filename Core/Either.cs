using System;

namespace Mio
{
    public struct Either<L, R>
    {
        internal L Left { get; }
        internal R Right { get; }

        private bool IsRight { get; }
        private bool IsLeft => !IsRight;

        internal Either(L left)
        {
            IsRight = false;
            Left = left;
            Right = default(R);
        }

        internal Either(R right)
        {
            IsRight = true;
            Right = right;
            Left = default(L);
        }

        public TR Match<TR>(Func<L, TR> Left, Func<R, TR> Right)
            => IsLeft ? Left(this.Left) : Right(this.Right);

        public override string ToString() => Match(l => $"Left({l})", r => $"Right({r})");
    }

    public static class Either
    {
        public struct Left<L>
        {
            internal L Value { get; }
            internal Left(L value) { Value = value; }
            public override string ToString() => $"Left({Value})";
        }

        public struct Right<R>
        {
            internal R Value { get; }
            internal Right(R value) { Value = value; }
            public override string ToString() => $"Right({Value})";
        }
    }
}