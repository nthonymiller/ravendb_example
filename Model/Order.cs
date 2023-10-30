using NUlid;

namespace RavenExample.Model;

public class Order : Entity<OrderId>
{
  public string OrderNo { get; set; } = string.Empty;
  public required CustomerId CustomerId { get; set; }
  
  public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
  public required ProductId ProductId { get; set; }
  public required int Quantity { get; set; }
}

public sealed record OrderId(string Value) : StronglyTypedId<string>(Value)
{
  public static OrderId Create() => new(Ulid.NewUlid().ToString());
  public override string ToString() => Value;
}
