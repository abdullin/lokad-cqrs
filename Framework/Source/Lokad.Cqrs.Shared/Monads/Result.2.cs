#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary>
	/// Improved version of the Result[T], that could serve as a basis for it.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <typeparam name="TError">The type of the error.</typeparam>
	/// <remarks>It is to be moved up-stream if found useful in other projects.</remarks>
	public sealed class Result<TValue, TError> : IEquatable<Result<TValue, TError>>
	{
		readonly bool _isSuccess;
		readonly TValue _value;
		readonly TError _error;

		Result(bool isSuccess, TValue value, TError error)
		{
			_isSuccess = isSuccess;
			_value = value;
			_error = error;
		}

		/// <summary>
		/// Creates the success result.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>result encapsulating the success value</returns>
		/// <exception cref="ArgumentNullException">if value is a null reference type</exception>
		public static Result<TValue, TError> CreateSuccess(TValue value)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (null == value) throw new ArgumentNullException("value");
			// ReSharper restore CompareNonConstrainedGenericWithNull

			return new Result<TValue, TError>(true, value, default(TError));
		}

		/// <summary>
		/// Creates the error result.
		/// </summary>
		/// <param name="error">The error.</param>
		/// <returns>result encapsulating the error value</returns>
		/// <exception cref="ArgumentNullException">if error is a null reference type</exception>
		public static Result<TValue, TError> CreateError(TError error)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (null == error) throw new ArgumentNullException("error");
			// ReSharper restore CompareNonConstrainedGenericWithNull

			return new Result<TValue, TError>(false, default(TValue), error);
		}

		/// <summary>
		/// item associated with this result
		/// </summary>
		public TValue Value
		{
			get
			{
				if (!_isSuccess)
					throw Errors.InvalidOperation(ResultResources.Dont_access_result_on_error_X, _error);

				return _value;
			}
		}

		/// <summary>
		/// Error message associated with this failure
		/// </summary>
		public TError Error
		{
			get
			{
				if (_isSuccess)
					throw new InvalidOperationException(ResultResources.Dont_access_error_on_valid_result);
				
				return _error;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this result is valid.
		/// </summary>
		/// <value><c>true</c> if this result is valid; otherwise, <c>false</c>.</value>
		public bool IsSuccess
		{
			get { return _isSuccess; }
		}


		/// <summary>
		/// Performs an implicit conversion from <typeparamref name="TValue"/> to <see cref="Result{TValue,TError}"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		/// <exception cref="ArgumentNullException">If value is a null reference type</exception>
		public static implicit operator Result<TValue, TError>(TValue value)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (null == value) throw new ArgumentNullException("value");
			// ReSharper restore CompareNonConstrainedGenericWithNull
			return CreateSuccess(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <typeparamref name="TError"/> to <see cref="Result{TValue,TError}"/>.
		/// </summary>
		/// <param name="error">The error.</param>
		/// <returns>The result of the conversion.</returns>
		/// <exception cref="ArgumentNullException">If value is a null reference type</exception>
		public static implicit operator Result<TValue, TError>(TError error)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (null == error) throw new ArgumentNullException("error");
			// ReSharper restore CompareNonConstrainedGenericWithNull
			return CreateError(error);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(Result<TValue, TError> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other._isSuccess.Equals(_isSuccess) && Equals(other._value, _value) && Equals(other._error, _error);
		}

		/// <summary>
		/// Applies the specified <paramref name="action"/>
		/// to this <see cref="Result{T}"/>, if it has value.
		/// </summary>
		/// <param name="action">The action to apply.</param>
		/// <returns>returns same instance for inlining</returns>
		/// <exception cref="ArgumentNullException">if <paramref name="action"/> is null</exception>
		public Result<TValue,TError> Apply([NotNull] Action<TValue> action)
		{
			if (action == null) throw new ArgumentNullException("action");
			if (_isSuccess)
				action(_value);

			return this;
		}

		/// <summary>
		/// Handles the specified handler.
		/// </summary>
		/// <param name="handler">The handler.</param>
		/// <returns>same instance for the inlining</returns>
		public Result<TValue, TError> Handle([NotNull] Action<TError> handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			if (!_isSuccess)
				handler(_error);

			return this;
		}

		/// <summary>
		/// Converts value of this instance
		/// using the provided <paramref name="converter"/>
		/// </summary>
		/// <typeparam name="TTarget">The type of the target.</typeparam>
		/// <param name="converter">The converter.</param>
		/// <returns>Converted result</returns>
		/// <exception cref="ArgumentNullException"> if <paramref name="converter"/> is null</exception>
		public Result<TTarget, TError> Convert<TTarget>([NotNull] Func<TValue, TTarget> converter)
		{
			if (converter == null) throw new ArgumentNullException("converter");
			if (!_isSuccess)
				return Result<TTarget, TError>.CreateError(_error);

			return converter(_value);
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
			if (obj.GetType() != typeof (Result<TValue, TError>)) return false;
			return Equals((Result<TValue, TError>) obj);
		}


		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int result = _isSuccess.GetHashCode();
// ReSharper disable CompareNonConstrainedGenericWithNull
				result = (result*397) ^ (_value != null ? _value.GetHashCode() : 1);
				result = (result*397) ^ (_error != null ? _error.GetHashCode() : 0);
// ReSharper restore CompareNonConstrainedGenericWithNull
				return result;
			}
		}

		/// <summary>
		/// Combines this <see cref="Result{T}"/> with the result returned
		/// by <paramref name="converter"/>.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target.</typeparam>
		/// <param name="converter">The converter.</param>
		/// <returns>Combined result.</returns>
		public Result<TTarget, TError> Combine<TTarget>([NotNull] Func<TValue, Result<TTarget, TError>> converter)
		{
			if (converter == null) throw new ArgumentNullException("converter");
			if (!_isSuccess)
				return Result<TTarget, TError>.CreateError(_error);

			return converter(_value);
		}

		/// <summary>
		/// Converts this <see cref="Result{T}"/> to <see cref="Maybe{T}"/>, 
		/// using the <paramref name="converter"/> to perform the value conversion.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target.</typeparam>
		/// <param name="converter">The reflector.</param>
		/// <returns><see cref="Maybe{T}"/> that represents the original value behind the <see cref="Result{T}"/> after the conversion</returns>
		public Maybe<TTarget> ToMaybe<TTarget>([NotNull] Func<TValue, TTarget> converter)
		{
			if (converter == null) throw new ArgumentNullException("converter");
			if (!_isSuccess)
				return Maybe<TTarget>.Empty;

			return converter(_value);
		}

		/// <summary>
		/// Converts this <see cref="Result{T}"/> to <see cref="Maybe{T}"/>, 
		/// with the original value reference, if there is any.
		/// </summary>
		/// <returns><see cref="Maybe{T}"/> that represents the original value behind the <see cref="Result{T}"/>.</returns>
		public Maybe<TValue> ToMaybe()
		{
			if (!_isSuccess)
				return Maybe<TValue>.Empty;

			return _value;
		}

		/// <summary>
		/// Exposes result failure as the exception (providing compatibility, with the exception -expecting code).
		/// </summary>
		/// <param name="exception">The function to generate exception, provided the error string.</param>
		/// <returns>result value</returns>
		public TValue ExposeException([NotNull] Func<TError, Exception> exception)
		{
			if (exception == null) throw new ArgumentNullException("exception");
			if (!IsSuccess)
				throw exception(Error);

			// abdullin: we can return value here, since failure chain ends here
			return Value;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (!_isSuccess)
				return "<Error: '" + _error + "'>";

			return "<Value: '" + _value + "'>";
		}
	}
}