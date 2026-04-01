using System;

namespace K9M;

public partial struct ValueList<T>
{
    /// <summary>
    /// Returns a reference-type wrapper for this collection.
    /// </summary>
    /// <returns>A reference-type wrapper for this collection.</returns>
    public Box Wrap()
    {
        return new(this);
    }

    /// <summary>
    /// Represents a reference-type wrapper for ValueList&lt;T&gt; instances.
    /// </summary>
    public class Box : IEquatable<Box>
    {
        /// <summary>
        /// The ValueList&lt;T&gt; instance that is wrapped by this box. 
        /// </summary>
        public ValueList<T> List;

        /// <summary>
        /// Initializes a new box that wraps a ValueList&lt;T&gt; instance created with
        /// the parameterless constructor.
        /// </summary>
        public Box()
        {
            List = new();
        }

        /// <summary>
        /// Initializes a new box that wraps the specified ValueList&lt;T&gt; instance.
        /// </summary>
        /// <param name="list"></param>
        public Box(ValueList<T> list)
        {
            List = list;
        }

        /// <summary>
        /// Determines whether this and the other box are wrapping the same collection.
        /// </summary>
        /// <param name="other">The box to compare to this instance.</param>
        /// <returns>true if this and the other box are wrapping the same collection, otherwise false.</returns>
        public virtual bool Equals(Box? other)
        {
            return other is not null && List.Equals(other.List);
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueList<T>.Box other && Equals(other);
        }

        public override int GetHashCode()
        {
            return List.GetHashCode();
        }

        /// <summary>
        /// Returns the ValueList&lt;T&gt; instance that is wrapped by this box.
        /// </summary>
        /// <returns>The ValueList&lt;T&gt; instance that is wrapped by this box.</returns>
        public ValueList<T> Unwrap()
        {
            return List;
        }

        /// <summary>
        /// Determines whether the two specified boxes are wrapping the same ValueList&lt;T&gt; instance.
        /// </summary>
        /// <param name="left">The first box to compare.</param>
        /// <param name="right">The second box to compare.</param>
        /// <returns>true if the two boxs are equal, otherwise false.</returns>
        public static bool operator ==(Box left, Box right)
        {
            return left is not null ? left.Equals(right) : right is null;
        }

        /// <summary>
        /// Determines whether the two specified boxes are wrapping different lists.
        /// </summary>
        /// <param name="left">The first box to compare.</param>
        /// <param name="right">The second box to compare.</param>
        /// <returns>true if the two boxs are different, otherwise false.</returns>
        public static bool operator !=(Box left, Box right)
        {
            return !(left == right);
        }
    }
}
