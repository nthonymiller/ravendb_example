namespace RavenExample.Model;

public interface IEntity
{
  
}

public abstract class Entity<TId> : IEntity where TId : notnull
{
  public required TId Id { get; init; }
}