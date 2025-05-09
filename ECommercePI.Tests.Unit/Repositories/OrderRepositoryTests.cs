using ECommercePI.Domain.Entities;
using ECommercePI.Infrastructure;
using ECommercePI.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommercePI.Tests.Unit.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        // 🧠 SQLite in-memory bağlantı oluşturuluyor
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated(); // Tabloları oluştur

        _repository = new OrderRepository(_context);
    }

    [Fact]
    public async Task SaveAsync_Should_Add_Order_To_Database()
    {
        var order = new Order
        {
            OrderId = "test-1",
            Amount = 100,
            Status = "created",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderItemId = "item-1",
                    ProductId = "prod-001",
                    Quantity = 2,
                    Price = 50
                }
            }
        };

        await _repository.SaveAsync(order);

        var saved = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == "test-1");

        saved.Should().NotBeNull();
        saved!.Items.Count.Should().Be(1);
        saved.Items[0].ProductId.Should().Be("prod-001");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        var result = await _repository.GetByIdAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Order()
    {
        var order = new Order
        {
            OrderId = "test-2",
            Amount = 200,
            Status = "created",
            CreatedAt = DateTime.UtcNow,
            Items = []
        };

        await _repository.SaveAsync(order);

        order.Status = "completed";
        order.CompletedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(order);

        var updated = await _repository.GetByIdAsync("test-2");

        updated!.Status.Should().Be("completed");
        updated.CompletedAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose(); // 🧼 Bağlantı kapatılır ve veritabanı yok olur
    }
}
