#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Helper class that indicates nullable value in a good-citizenship code
    /// </summary>
    /// <typeparam name="T">underlying type</typeparam>
    [Serializable]
    public sealed class Maybe<T> : IEquatable<Maybe<T>> 
    {
        readonly T _value;
        readonly bool _hasValue;

        Maybe(T item, bool hasValue)
        {
            _value = item;
            _hasValue = hasValue;
        }

        Maybe(T value)
            : this(value, true)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            // ReSharper restore CompareNonConstrainedGenericWithNull
        }

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T), false);

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    throw new InvalidOperationException("Dont access value when maybe is empty");
                }

                return _value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Retrieves value from this instance, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public T GetValue(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        /// <summary>
        /// Retrieves value from this instance, using a <paramref name="defaultValue"/>
        /// factory, if it is absent
        /// </summary>
        /// <param name="defaultValue">The default value to provide.</param>
        /// <returns>maybe value</returns>
        public Maybe<T> GetValue(Func<Maybe<T>> defaultValue)
        {
            return _hasValue ? this : defaultValue();
        }
     

        /// <summary>
        /// Throws the exception if maybe does not have value.
        /// </summary>
        /// <returns>actual value</returns>
        /// <exception cref="InvalidOperationException">if maybe does not have value</exception>
        public T ExposeException(string message, params object[] args)
        {
            if (message == null) throw new ArgumentNullException(@"message");
            if (!_hasValue)
            {
                var text = string.Format(message, args);
                throw new InvalidOperationException(text);
            }

            return _value;
        }

        /// <summary>
        /// Converts this instance to <see cref="Maybe{T}"/>, 
        /// while applying <paramref name="converter"/> if there is a value.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="converter">The converter.</param>
        /// <returns></returns>
        public Maybe<TTarget> Convert<TTarget>(Func<T, TTarget> converter)
        {
            return _hasValue ? converter(_value) : Maybe<TTarget>.Empty;
        }

        /// <summary>
        /// Retrieves converted value, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <typeparam name="TTarget">type of the conversion target</typeparam>
        /// <param name="converter">The converter.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public TTarget Convert<TTarget>(Func<T, TTarget> converter, Func<TTarget> defaultValue)
        {
            return _hasValue ? converter(_value) : defaultValue();
        }

        /// <summary>
        /// Retrieves converted value, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <typeparam name="TTarget">type of the conversion target</typeparam>
        /// <param name="converter">The converter.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public TTarget Convert<TTarget>(Func<T, TTarget> converter, TTarget defaultValue)
        {
            return _hasValue ? converter(_value) : defaultValue;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Maybe{T}"/> is equal to the current <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybe">The <see cref="Maybe"/> to compare with.</param>
        /// <returns>true if the objects are equal</returns>
        public bool Equals(Maybe<T> maybe)
        {
            if (ReferenceEquals(null, maybe)) return false;
            if (ReferenceEquals(this, maybe)) return true;

            if (_hasValue != maybe._hasValue) return false;
            if (!_hasValue) return true;
            return _value.Equals(maybe._value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var maybe = obj as Maybe<T>;
            if (maybe == null) return false;
            return Equals(maybe);
        }

        /// <summary>
        /// Serves as a hash function for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Maybe{T}"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable CompareNonConstrainedGenericWithNull
                return ((_value != null ? _value.GetHashCode() : 0)*397) ^ _hasValue.GetHashCode();
                // ReSharper restore CompareNonConstrainedGenericWithNull
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Performs an implicit conversion from <typeparamref name="T"/> to <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Maybe<T>(T item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (item == null) throw new ArgumentNullException("item");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return new Maybe<T>(item);
        }



        /// <summary>
        /// Performs an explicit conversion from <see cref="Maybe{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator T(Maybe<T> item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (!item.HasValue) throw new ArgumentException("May be must have value");

            return item.Value;
        }



        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_hasValue)
            {
                return "<" + _value + ">";
            }

            return "<Empty>";
        }
    }
}