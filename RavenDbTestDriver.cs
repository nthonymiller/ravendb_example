using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using RavenExample.Model;

namespace RavenExample;

public class RavenDbTestDriver : RavenTestDriver
{
  private static bool IsInitialized { get; set; }
  
  public RavenDbTestDriver()
  {
    if (!IsInitialized)
    {
      ConfigureServer(new TestServerOptions
      {
        DataDirectory = Path.Join(Path.GetTempPath(), "RavenDb", "TestDriver")
      });
      IsInitialized = true;
    }
  }

  protected override void PreInitialize(IDocumentStore documentStore)
  {
    documentStore.Conventions.MaxNumberOfRequestsPerSession = 100;
    documentStore.Conventions.Serialization = RavenDbHelpers.CreateCustomJsonSerialization();
    documentStore.Conventions.FindIdentityProperty = RavenDbHelpers.FindIdentityProperty();
    documentStore.Conventions.AsyncDocumentIdGenerator = RavenDbHelpers.AsyncDocumentIdGenerator;
  }

  public IDocumentStore GetStore(
    IReadOnlyList<IEntity>? entities = default,
    IReadOnlyList<IAbstractIndexCreationTask>? indexes = default)
  {
    var store = GetDocumentStore();
    store.OnAfterConversionToEntity += RavenDbHelpers.StoreAfterConversionToEntity;
    store.OnBeforeStore += RavenDbHelpers.StoreOnBeforeStore;
    
    if (indexes != null && indexes.Any())
    {
      store.ExecuteIndexes(indexes);
    }
    
    if (entities != null && entities.Any())
    {
      using var docSession = store.OpenSession();
      foreach (var entity in entities)
      {
        docSession.Store(entity);
      }
      docSession.SaveChanges();
    }
    
    // If we want to query documents, sometimes we need to wait for the indexes to catch up  
    // to prevent using stale indexes.
    WaitForIndexing(store);

    // Sometimes we want to debug the test itself. This method redirects us to the studio
    // so that we can see if the code worked as expected
    WaitForUserToContinueTheTest(store);

    return store;
  }
}