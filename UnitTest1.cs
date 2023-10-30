using FluentAssertions;
using Raven.Client.Documents;
using RavenExample.Model;

namespace RavenExample;

public class UnitTest1
{
  
  [Fact]
  public async Task ShouldPersistProduct()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore();
    var session = store.OpenAsyncSession();
    
    await session.StoreAsync(product);
    await session.SaveChangesAsync();

    var session1 = store.OpenAsyncSession();
    var product1 = await session1.LoadAsync<Product>(product.Id.ToString());

    product1.Should().NotBeNull();
    product1.Id.Should().Be(product.Id);
    product1.Name.Should().Be(product.Name);
  }

  [Fact]
  public async Task ShouldQueryProductById()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore(new [] { product });
    var session = store.OpenAsyncSession();

    var query = session.Query<Product>()
      .Where(p => p.Id == product.Id);

    var product1 = await query.FirstOrDefaultAsync();

    product1.Should().NotBeNull();
    product1.Id.Should().Be(product.Id);
    product1.Name.Should().Be(product.Name);
  }
  
  [Fact]
  public async Task ShouldQueryProductByName()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore(new [] { product });
    var session = store.OpenAsyncSession();

    var query = session.Query<Product>()
      .Where(p => p.Name == product.Name);

    var product1 = await query.FirstOrDefaultAsync();

    product1.Should().NotBeNull();
    product1.Id.Should().Be(product.Id);
    product1.Name.Should().Be(product.Name);
  }
  
  [Fact]
  public async Task ShouldPersistOrder()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };

    var order = new Order()
    {
      Id = OrderId.Create(),
      OrderNo = "123",
      CustomerId = CustomerId.Create(),
      Lines = new List<OrderLine>()
      {
        new() { ProductId = product.Id, Quantity = 1 }
      }
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore(new []{product});
    var session = store.OpenAsyncSession();
    
    await session.StoreAsync(order);
    await session.SaveChangesAsync();

    var session1 = store.OpenAsyncSession();
    var order1 = await session1.LoadAsync<Order>(order.Id.ToString());

    order1.Should().NotBeNull();
    order1.Id.Should().Be(order.Id);
    order1.OrderNo.Should().Be(order.OrderNo);

    order1.Lines.Should().NotBeEmpty();
    order1.Lines.First().ProductId.Should().Be(product.Id);
    order1.Lines.First().Quantity.Should().Be(1);
  }
  
  [Fact]
  public async Task ShouldLoadOrder()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };

    var order = new Order()
    {
      Id = OrderId.Create(),
      CustomerId = CustomerId.Create(),
      OrderNo = "123",
      Lines = new List<OrderLine>()
      {
        new() { ProductId = product.Id, Quantity = 1 }
      }
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore(new IEntity[]{product, order});
    var session = store.OpenAsyncSession();
    
    await session.StoreAsync(order);
    await session.SaveChangesAsync();

    var order1 = await session.LoadAsync<Order>(order.Id.ToString());

    order1.Should().NotBeNull();
    order1.Id.Should().Be(order.Id);
    order1.OrderNo.Should().Be(order.OrderNo);

    order1.Lines.Should().NotBeEmpty();
    order1.Lines.First().ProductId.Should().Be(product.Id);
    order1.Lines.First().Quantity.Should().Be(1);
  }

  [Fact]
  public async Task ShouldLoadOrderAndIncludeProducts()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };

    var order = new Order()
    {
      Id = OrderId.Create(),
      CustomerId = CustomerId.Create(),
      OrderNo = "123",
      Lines = new List<OrderLine>()
      {
        new() { ProductId = product.Id, Quantity = 1 }
      }
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore(new IEntity[]{product, order});
    var session = store.OpenAsyncSession();
    
    await session.StoreAsync(order);
    await session.SaveChangesAsync();

    // Product is not included
    var order1 = await session.Include<Order>(x => x.Lines.Select(l => l.ProductId.Value)).LoadAsync<Order>(order.Id.ToString());

    order1.Should().NotBeNull();
    order1.Id.Should().Be(order.Id);
    order1.OrderNo.Should().Be(order.OrderNo);

    order1.Lines.Should().NotBeEmpty();
    order1.Lines.First().ProductId.Should().Be(product.Id);
    order1.Lines.First().Quantity.Should().Be(1);
    
    // This will make a subsequent call to RavenDb to fetch the product
    var product1 = await session.LoadAsync<Product>(order1.Lines.First().ProductId.ToString());
  }
  
  [Fact]
  public async Task ShouldLoadOrderAndIncludeCustomer()
  {
    var product = new Product
    {
      Id = ProductId.Create(),
      Name = "Test"
    };

    var customer = new Customer
    {
      Id = CustomerId.Create(),
      Name = "My Customer"
    };

    var order = new Order()
    {
      Id = OrderId.Create(),
      CustomerId = customer.Id,
      OrderNo = "123",
      Lines = new List<OrderLine>()
      {
        new() { ProductId = product.Id, Quantity = 1 }
      }
    };
    
    var ravenDb = new RavenDbTestDriver();
    var store = ravenDb.GetStore(new IEntity[]{product, order, customer});
    var session = store.OpenAsyncSession();
    
    await session.StoreAsync(order);
    await session.SaveChangesAsync();

    var order1 = await session.Include<Order>(x => x.CustomerId.Value).LoadAsync<Order>(order.Id.ToString());

    order1.Should().NotBeNull();
    order1.Id.Should().Be(order.Id);
    order1.OrderNo.Should().Be(order.OrderNo);

    order1.Lines.Should().NotBeEmpty();
    order1.Lines.First().ProductId.Should().Be(product.Id);
    order1.Lines.First().Quantity.Should().Be(1);
  }
  
}