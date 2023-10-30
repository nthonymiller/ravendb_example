namespace RavenExample.Model;

public abstract record StronglyTypedId<TValue>(TValue Value)
	where TValue : notnull
{
	public override string ToString() => Value.ToString()!;
}