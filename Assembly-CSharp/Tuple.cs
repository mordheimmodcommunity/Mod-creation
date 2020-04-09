using System;

public static class Tuple
{
	public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 second)
	{
		return new Tuple<T1, T2>(item1, second);
	}

	public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 second, T3 third)
	{
		return new Tuple<T1, T2, T3>(item1, second, third);
	}

	public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 second, T3 third, T4 fourth)
	{
		return new Tuple<T1, T2, T3, T4>(item1, second, third, fourth);
	}
}
public sealed class Tuple<T1, T2>
{
	private readonly T1 item1;

	private readonly T2 item2;

	public T1 Item1 => item1;

	public T2 Item2 => item2;

	public Tuple(T1 item1, T2 item2)
	{
		this.item1 = item1;
		this.item2 = item2;
	}

	public override string ToString()
	{
		return $"Tuple({Item1}, {Item2})";
	}

	public override int GetHashCode()
	{
		int num = 17;
		num = num * 23 + item1.GetHashCode();
		return num * 23 + item2.GetHashCode();
	}

	public override bool Equals(object o)
	{
		if ((object)o.GetType() != typeof(Tuple<T1, T2>))
		{
			return false;
		}
		Tuple<T1, T2> tuple = (Tuple<T1, T2>)o;
		return this == tuple;
	}

	public void Unpack(Action<T1, T2> unpackerDelegate)
	{
		unpackerDelegate(Item1, Item2);
	}
}
public sealed class Tuple<T1, T2, T3>
{
	private readonly T1 item1;

	private readonly T2 item2;

	private readonly T3 item3;

	public T1 Item1 => item1;

	public T2 Item2 => item2;

	public T3 Item3 => item3;

	public Tuple(T1 item1, T2 item2, T3 item3)
	{
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
	}

	public override int GetHashCode()
	{
		int num = 17;
		num = num * 23 + item1.GetHashCode();
		num = num * 23 + item2.GetHashCode();
		return num * 23 + item3.GetHashCode();
	}

	public override bool Equals(object o)
	{
		if ((object)o.GetType() != typeof(Tuple<T1, T2, T3>))
		{
			return false;
		}
		Tuple<T1, T2, T3> tuple = (Tuple<T1, T2, T3>)o;
		return this == tuple;
	}

	public void Unpack(Action<T1, T2, T3> unpackerDelegate)
	{
		unpackerDelegate(Item1, Item2, Item3);
	}
}
public sealed class Tuple<T1, T2, T3, T4>
{
	private readonly T1 item1;

	private readonly T2 item2;

	private readonly T3 item3;

	private readonly T4 item4;

	public T1 Item1 => item1;

	public T2 Item2 => item2;

	public T3 Item3 => item3;

	public T4 Item4 => item4;

	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
	}

	public override int GetHashCode()
	{
		int num = 17;
		num = num * 23 + item1.GetHashCode();
		num = num * 23 + item2.GetHashCode();
		num = num * 23 + item3.GetHashCode();
		return num * 23 + item4.GetHashCode();
	}

	public override bool Equals(object o)
	{
		if ((object)o.GetType() != typeof(Tuple<T1, T2, T3, T4>))
		{
			return false;
		}
		Tuple<T1, T2, T3, T4> tuple = (Tuple<T1, T2, T3, T4>)o;
		return this == tuple;
	}

	public void Unpack(Action<T1, T2, T3, T4> unpackerDelegate)
	{
		unpackerDelegate(Item1, Item2, Item3, Item4);
	}
}
