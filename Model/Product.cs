using NUlid;

namespace RavenExample.Model;

public class Product : Entity<ProductId>
{
  public required string Name { get; set; }
}

public sealed record ProductId(string Value) : StronglyTypedId<string>(Value)
{
  public static ProductId Create() => new(Ulid.NewUlid().ToString());
  public override string ToString() => Value;
}