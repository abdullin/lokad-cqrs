using ProtoBuf;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.SqlViews.Tests
{
	[ProtoContract]
	public sealed class TestView
	{
		[ProtoMember(1)]
		public string Value { get; set; }

		public bool Equals(TestView other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Value, Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (TestView)) return false;
			return Equals((TestView) obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format("Value: {0}", Value);
		}
	}

	[ProtoContract]
	public sealed class AnotherView
	{
		[ProtoMember(1)]
		public string Value { get; set; }
	}

}