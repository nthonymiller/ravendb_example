using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace RavenExample.Model;

public class StronglyTypedIdHelper
{
	private static readonly ConcurrentDictionary<Type, Delegate> StronglyTypedIdFactories = new();
	private static readonly ConcurrentDictionary<Type, PropertyInfo?> StronglyTypedIdEntities = new();

	public static Func<TValue, object> GetFactory<TValue>(Type stronglyTypedIdType)
		where TValue : notnull
	{
		return (Func<TValue, object>)StronglyTypedIdFactories.GetOrAdd(
			stronglyTypedIdType,
			CreateFactory<TValue>);
	}

	private static Func<TValue, object> CreateFactory<TValue>(Type stronglyTypedIdType)
		where TValue : notnull
	{
		if (!IsStronglyTypedId(stronglyTypedIdType))
			throw new ArgumentException($"Type '{stronglyTypedIdType}' is not a strongly-typed id type",
				nameof(stronglyTypedIdType));

		var ctor = stronglyTypedIdType.GetConstructor(new[] { typeof(TValue) });
		if (ctor is null)
			throw new ArgumentException(
				$"Type '{stronglyTypedIdType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'",
				nameof(stronglyTypedIdType));

		var param = Expression.Parameter(typeof(TValue), "Value");
		var body = Expression.New(ctor, param);
		var lambda = Expression.Lambda<Func<TValue, object>>(body, param);
		return lambda.Compile();
	}

	public static bool IsStronglyTypedId(Type type) => IsStronglyTypedId(type, out _);

	public static bool IsStronglyTypedId(Type type, [NotNullWhen(true)] out Type? idType)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		if (type.BaseType is { IsGenericType: true } baseType &&
		    baseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
		{
			idType = baseType.GetGenericArguments()[0];
			return true;
		}

		idType = default;
		return false;
	}

	public static bool IsStronglyTypedEntity(Type type) => IsStronglyTypedEntity(type, out PropertyInfo? _);

	public static bool IsStronglyTypedEntity(Type type, out Type? propertyType, out PropertyInfo? idPropInfo)
	{
		if (IsStronglyTypedEntity(type, out idPropInfo) && idPropInfo is not null)
		{
			propertyType = idPropInfo.PropertyType;
			return true;
		}

		propertyType = default;

		return false;
	}

	public static bool IsStronglyTypedEntity(Type type, out PropertyInfo? idPropInfo)
	{
		idPropInfo = StronglyTypedIdEntities.GetOrAdd(type, typeArg =>
		{
			var entityIdentifierProperty = type.GetProperty("Id");

			var stronglyTypedEntity = entityIdentifierProperty?.PropertyType != null &&
			                          IsStronglyTypedId(entityIdentifierProperty.PropertyType);

			var idPropInfo = stronglyTypedEntity ? entityIdentifierProperty : default;
			return idPropInfo;
		});

		return idPropInfo is not null;
	}
}