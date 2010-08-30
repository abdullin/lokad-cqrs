#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#if DEBUG && !SILVERLIGHT2

namespace Lokad.Reflection
{
	static class ILHelper
	{
		static OpCode[] _multiByteCodes;
		static OpCode[] _singleByteCodes;

		static ILHelper()
		{
			LoadOpCodes();
		}

		static void LoadOpCodes()
		{
			_singleByteCodes = new OpCode[0x100];
			_multiByteCodes = new OpCode[0x100];
			var fields = typeof (OpCodes)
				.GetFields()
				.Where(i => i.FieldType == typeof (OpCode));

			foreach (var info in fields)
			{
				if (info.FieldType == typeof (OpCode))
				{
					var code = (OpCode) info.GetValue(null);
					var value = (ushort) code.Value;
					if (value < 0x100)
					{
						_singleByteCodes[value] = code;
					}
					else
					{
						if ((value & 0xff00) != 0xfe00)
						{
							throw new InvalidOperationException("Invalid OpCode.");
						}
						_multiByteCodes[value & 0xff] = code;
					}
				}
			}
		}

		internal sealed class ILInstruction<T> : ILInstruction
		{
			readonly OpCode _code;
			readonly T _operand;
			readonly long _position;

			public ILInstruction(OpCode code, T operand, long position)
			{
				_code = code;
				_operand = operand;
				_position = position;
			}

			public override string ToString()
			{
				return string.Format("{2:000} Code: {0}, Operand: {1} ({3})", _code, _operand, _position, _operand.GetType().Name);
			}
		}


		public static ILInstruction<T> From<T>(OpCode item, T operand, long position)
		{
			return new ILInstruction<T>(item, operand, position);
		}

		public abstract class ILInstruction
		{
		}


		static ILInstruction ReadInstruction(BinaryReader il, Module module, Type[] genericTypeArguments,
			Type[] genericMethodArguments)
		{
			OpCode code;

			var position = il.BaseStream.Position;
			ushort value = il.ReadByte();

			if (value != 0xfe)
			{
				code = _singleByteCodes[value];
			}
			else
			{
				value = il.ReadByte();
				code = _multiByteCodes[value];
			}

			switch (code.OperandType)
			{
				case OperandType.InlineBrTarget:
					return From(code, il.ReadInt32(), position);
				case OperandType.InlineField:
					return From(code, module.ResolveField(il.ReadInt32(), genericTypeArguments, genericMethodArguments), position);
				case OperandType.InlineMethod:
					try
					{
						return From(code, module.ResolveMethod(il.ReadInt32(), genericTypeArguments, genericMethodArguments), position);
					}
					catch
					{
						return From(code, module.ResolveMember(il.ReadInt32()), position);
					}
				case OperandType.InlineSig:
					return From(code, module.ResolveSignature(il.ReadInt32()), position);
				case OperandType.InlineType:
					return From(code, module.ResolveType(il.ReadInt32(), genericTypeArguments, genericMethodArguments), position);
					//instruction.Operand = module.ResolveType(metadataToken, this.mi.DeclaringType.GetGenericArguments(), this.mi.GetGenericArguments());
				case OperandType.InlineI:
					return From(code, il.ReadInt32(), position);
				case OperandType.InlineI8:
					return From(code, il.ReadInt64(), position);
				case OperandType.InlineNone:
					return From(code, 0, position);
				case OperandType.InlineR:
					return From(code, il.ReadDouble(), position);
				case OperandType.InlineString:
					var stringToken = il.ReadInt32();
					return From(code, module.ResolveString(stringToken), position);
				case OperandType.InlineSwitch:
					int count = il.ReadInt32();
					var addresses = Range.Array(count, i => il.ReadInt32());
					return From(code, addresses, position);
				case OperandType.InlineVar:
					return From(code, il.ReadUInt16(), position);
				case OperandType.ShortInlineBrTarget:
					return From(code, il.ReadSByte(), position);
				case OperandType.ShortInlineI:
					return From(code, il.ReadSByte(), position);
				case OperandType.ShortInlineR:
					return From(code, il.ReadSingle(), position);
				case OperandType.ShortInlineVar:
					return From(code, il.ReadByte(), position);
				default:
					throw new InvalidOperationException(code.OperandType.ToString());
			}
		}


		public static ILInstruction[] Read(Delegate d)
		{
			var method = d.Method;
			var il = method.GetMethodBody().GetILAsByteArray();

			using (var stream = new MemoryStream(il))
			using (var reader = new BinaryReader(stream))
			{
				var list = new List<ILInstruction>();
				while (stream.Position < il.Length)
				{
					list.Add(ReadInstruction(reader, method.Module, method.DeclaringType.GetGenericArguments(),
						method.GetGenericArguments()));
				}
				return list.ToArray();
			}
		}
	}
}

#endif