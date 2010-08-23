#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Reflection;
using System.Reflection.Emit;
using Lokad.Quality;

namespace Lokad.Reflection
{
	/// <summary>
	/// Helper class for the IL-based strongly-typed reflection
	/// </summary>
	/// <remarks>This class is not supported by Silverlight 2.0, yet</remarks>
	public static class Reflect
	{
		static readonly byte Ldarg_0 = (byte) OpCodes.Ldarg_0.Value;
		static readonly byte Ldfld = (byte) OpCodes.Ldfld.Value;
		static readonly byte Ldflda = (byte) OpCodes.Ldflda.Value;
		static readonly byte Constrained = (byte) OpCodes.Constrained.Value;
		static readonly byte Stloc_0 = (byte) OpCodes.Stloc_0.Value;
		static readonly byte CallVirt = (byte) OpCodes.Callvirt.Value;
		static readonly byte Ret = (byte) OpCodes.Ret.Value;

		static Maybe<string> MemberNameSafely<T>(Func<T> expression)
		{
#if SILVERLIGHT2
			return Maybe<string>.Empty;
#else
			return Member(expression).Combine(x => GetNameForMember(x));
#endif
		}

		internal static string MemberName<T>(Func<T> expression)
		{
			return MemberNameSafely(expression).GetValue(ReflectCache<T>.ReferenceName);
		}

		static Maybe<string> GetNameForMember(MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					return member.Name;
				case MemberTypes.Method:
					var name = member.Name;
					if (!name.StartsWith("get_"))
						return Maybe<string>.Empty;

					return name.Remove(0, 4);
				default:
					return Maybe<string>.Empty;
			}
		}

		internal static string VariableName<T>(Func<T> expression)
		{
			return VariableNameSafely(expression).GetValue(ReflectCache<T>.ReferenceName);
		}


		static Maybe<string> VariableNameSafely<T>(Func<T> expression)
		{
#if SILVERLIGHT2
			return Maybe<string>.Empty;
#else
			return VariableSafely(expression)
				.Convert(e => e.Name);
#endif
		}


#if !SILVERLIGHT2

		/// <summary>
		/// Retrieves via IL the information of the <b>local</b> variable passed in the expression.
		/// <code>
		/// var myVar = "string";
		/// var info = Reflect.Variable(() =&gt; myVar)
		/// </code>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression">The expression containing the local variable to reflect.</param>
		/// <returns>information about the variable (if able to retrieve)</returns>
		/// <exception cref="ReflectLambdaException">if the provided expression is not a simple variable reference</exception>
		public static FieldInfo Variable<T>(Func<T> expression)
		{
			return VariableSafely(expression)
				.ExposeException(() => new ReflectLambdaException("Expected simple variable reference"));
		}

		/// <summary>
		/// Retrieves via IL the information of the <b>local</b> variable passed in the expression.
		/// <code>
		/// var myVar = "string";
		/// var info = Reflect.Variable(() =&gt; myVar)
		/// </code>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression">The expression containing the local variable to reflect.</param>
		/// <returns>information about the variable (if able to retrieve)</returns>
		public static Maybe<FieldInfo> VariableSafely<T>([NotNull] Func<T> expression)
		{
			if (expression == null) throw new ArgumentNullException("expression");
			var method = expression.Method;
			var body = method.GetMethodBody();
			if (null == body) return Maybe<FieldInfo>.Empty;
			var il = body.GetILAsByteArray();
			// in DEBUG we end up with stack
			// in release, there is a ret at the end
			if ((il[0] == Ldarg_0) && (il[1] == Ldfld) && ((il[6] == Stloc_0) || (il[6] == Ret)))
			{
				var fieldHandle = BitConverter.ToInt32(il, 2);

				var module = method.Module;
				var expressionType = expression.Target.GetType();

				if (!expressionType.IsGenericType)
				{
					return module.ResolveField(fieldHandle);
				}
				var genericTypeArguments = expressionType.GetGenericArguments();
				// method does not have any generics
				//var genericMethodArguments = method.GetGenericArguments();
				return module.ResolveField(fieldHandle, genericTypeArguments, Type.EmptyTypes);
			}
			return Maybe<FieldInfo>.Empty;
		}

		/// <summary>
		/// Retrieves via IL the <em>getter method</em> for the property being reflected.
		/// <code>
		/// var i2 = new
		/// {
		///   MyProperty = "Value"
		/// }; 
		/// var info = Reflect.Property(() => i2.Property);
		/// // info will have name of "get_MyProperty"
		/// </code>
		/// </summary>
		/// <typeparam name="T">type of the property to reflect</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>getter method for the property.</returns>
		/// <exception cref="ReflectLambdaException">if reference is not a simple property</exception>
		public static MethodInfo Property<T>(Func<T> expression)
		{
			return PropertySafely(expression)
				.ExposeException(() => new ReflectLambdaException("Expected simple property reference"));
		}

		/// <summary>
		/// Retrieves via IL the <em>getter method</em> for the property being reflected.
		/// <code>
		/// var i2 = new
		/// {
		///   MyProperty = "Value"
		/// }; 
		/// var info = Reflect.Property(() => i2.Property);
		/// // info will have name of "get_MyProperty"
		/// </code>
		/// </summary>
		/// <typeparam name="T">type of the property to reflect</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>getter method for the property.</returns>
		public static Maybe<MethodInfo> PropertySafely<T>(Func<T> expression)
		{
			return DelegatedPropertySafely(expression);
		}

		static Maybe<MemberInfo> Member<T>(Func<T> expression)
		{
			return DelegatedMember(expression);
		}

		// in DEBUG we end up with stack
		// in release, there is a ret at the end
		static Maybe<MethodInfo> DelegatedPropertySafely(Delegate expression)
		{
			var method = expression.Method;
			var body = method.GetMethodBody();
			if (null == body) return Maybe<MethodInfo>.Empty;
			var il = body.GetILAsByteArray();

			if ((il[0] == Ldarg_0) && (il[1] == Ldfld) && (il[6] == CallVirt) && ((il[11] == Stloc_0) || (il[11] == Ret)))
			{
				var getHandle = BitConverter.ToInt32(il, 7);

				return (MethodInfo) method.Module.ResolveMethod(getHandle);
			}
			if ((il[0] == Ldarg_0) && (il[1] == Ldflda) && (il[6] == 0xfe) && (il[7] == Constrained) && (il[12] == CallVirt) &&
				((il[17] == Stloc_0) || (il[17] == Ret)))
			{
				var getHandle = BitConverter.ToInt32(il, 13);

				return (MethodInfo) method.Module.ResolveMethod(getHandle);
			}
			return Maybe<MethodInfo>.Empty;
			//throw new ArgumentException("Expected simple property reference");
		}

		static Maybe<MemberInfo> DelegatedMember(Delegate expression)
		{
			var method = expression.Method;
			var body = method.GetMethodBody();
			if (null == body) return Maybe<MemberInfo>.Empty;
			var il = body.GetILAsByteArray();

			if ((il[0] == Ldarg_0) && (il[1] == Ldfld) && ((il[6] == CallVirt) || (il[6] == Ldfld)) &&
				((il[11] == Stloc_0) || (il[11] == Ret)))
			{
				var getHandle = BitConverter.ToInt32(il, 7);

				return method.Module.ResolveMember(getHandle);
			}
			return Maybe<MemberInfo>.Empty;
			//throw new ArgumentException("Expected simple member reference");
		}

#endif
	}
}