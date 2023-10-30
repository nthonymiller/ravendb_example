using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Raven.Client.Json.Serialization;
using Raven.Client.Json.Serialization.NewtonsoftJson;

namespace RavenExample;

public class CustomRavenDbContractResolver : DefaultRavenContractResolver
{
  private readonly HashSet<string> _ignoreProperties;
  
  public CustomRavenDbContractResolver(ISerializationConventions conventions, params string[] ignoreProperties) : base(conventions)
  {
    _ignoreProperties = new HashSet<string>(ignoreProperties);
  }
  
  protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
  {
    JsonProperty property = base.CreateProperty(member, memberSerialization);
    if (property.PropertyName is not null && _ignoreProperties.Contains(property.PropertyName))
    {
      property.ShouldSerialize = _ => false;
    }
    
    return property;
  }

}