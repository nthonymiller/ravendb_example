using NUlid;

namespace RavenExample.Model;

public class Customer : Entity<CustomerId>
{
  public required string Name { get; set; }
}

public sealed record CustomerId(string Value) : StronglyTypedId<string>(Value)
{
  public static CustomerId Create() => new(Ulid.NewUlid().ToString());
  public override string ToString() => Value;
}