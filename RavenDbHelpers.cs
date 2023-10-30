using System.Reflection;
using Newtonsoft.Json.Converters;
using NUlid;
using Raven.Client.Documents.Session;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using RavenExample.Model;

namespace RavenExample;

public static class RavenDbHelpers
{
  internal const string IdProperty = "Id";
  
  public static NewtonsoftJsonSerializationConventions CreateCustomJsonSerialization()
  {
    var result = new NewtonsoftJsonSerializationConventions
    {
      CustomizeJsonSerializer = serializer =>
      {
        serializer.Converters.Add(new StringEnumConverter());
        serializer.Converters.Add(new StronglyTypedIdNewtonsoftJsonConverter());
      },
    };
    result.JsonContractResolver = new CustomRavenDbContractResolver(result,  IdProperty);

    return result;
  }

  public static Func<MemberInfo, bool> FindIdentityProperty()
  {
    return memberInfo =>
    {
      if (memberInfo.DeclaringType is not null &&
          StronglyTypedIdHelper.IsStronglyTypedEntity(memberInfo.DeclaringType)) return false;

      return memberInfo.Name == "Id";
    };
  }

  public static void StoreAfterConversionToEntity(object? sender, AfterConversionToEntityEventArgs args)
  {
    if (StronglyTypedIdHelper.IsStronglyTypedEntity(args.Entity.GetType(), out var idType, out var idPropInfo))
    {
      var idValue = StronglyTypedIdHelper.GetFactory<string>(idType!)(args.Id);
      idPropInfo!.SetValue(args.Entity, idValue);
    }
  }

  public static void StoreOnBeforeStore(object? sender, BeforeStoreEventArgs args)
  {
    if (StronglyTypedIdHelper.IsStronglyTypedEntity(args.Entity.GetType(), out var idType, out var idPropInfo) &&
        idPropInfo!.GetValue(args.Entity) is null)
    {
      var idValue = StronglyTypedIdHelper.GetFactory<string>(idType!)(args.DocumentId);
      idPropInfo.SetValue(args.Entity, idValue);
    }
  }

  public static Task<string> AsyncDocumentIdGenerator(string dbName, object entity)
  {
    if (!StronglyTypedIdHelper.IsStronglyTypedEntity(entity.GetType(), out var idPropInfo) || idPropInfo is null)
      // Generate new Unique Id when Id is not strongly typed or IdProperty is not found
      return Task.FromResult(Ulid.NewUlid().ToString());

    // Get Id value from Entity
    var value = idPropInfo.GetValue(entity);
    if (value is not null) return Task.FromResult(value.ToString())!;

    // Generate new Unique Id when Id is null, using StronglyTypedIdFactory
    var id = StronglyTypedIdHelper.GetFactory<object>(entity.GetType())(entity);
    return Task.FromResult(id.ToString())!;
  }

}