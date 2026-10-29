using Moq;
using TradingCompany.Core.DTOs;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.BL.Tests;

[TestFixture]
public class DeliveryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IWaybillRepository> _waybillRepository = null!;
    private Mock<IShipmentRepository> _shipmentRepository = null!;
    private Mock<IShipmentStatusRepository> _statusRepository = null!;
    private Mock<TimeProvider> _timeProvider = null!;
    private UserSession _session = null!;
    private DeliveryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _waybillRepository = new Mock<IWaybillRepository>();
        _shipmentRepository = new Mock<IShipmentRepository>();
        _statusRepository = new Mock<IShipmentStatusRepository>();
        _timeProvider = new Mock<TimeProvider>();
        _timeProvider.Setup(t => t.GetUtcNow()).Returns(Now);
        _timeProvider.Setup(t => t.LocalTimeZone).Returns(TimeZoneInfo.Utc);

        _session = new UserSession();
        _session.SignIn(new UserDto { Id = 5, Login = "manager", IsDeliveryManager = true });

        _orderRepository.Setup(r => r.GetAllWithDeliveryAsync()).ReturnsAsync(() => new List<Order>
        {
            new() { Id = 1, OrderNumber = "ORD-1", CustomerId = 10, OrderDate = new DateTime(2026, 10, 1), TotalAmount = 100,
                    Customer = new User { Id = 10, FullName = "Іван Петренко", PhoneNumber = "+380" } },
            new() { Id = 2, OrderNumber = "ORD-2", CustomerId = 11, OrderDate = new DateTime(2026, 10, 5), TotalAmount = 300,
                    Customer = new User { Id = 11, FullName = "Анна Шевченко" },
                    Waybill = new Waybill
                    {
                        Id = 20, WaybillNumber = "WB-2", City = "Львів",
                        Shipments = { new Shipment { Id = 30, StatusId = ShipmentStatus.PendingId, Status = new ShipmentStatus { Id = 1, Name = "Очікує відправлення" } } }
                    } }
        });
        _statusRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ShipmentStatus>
        {
            new() { Id = 2, Name = "Відправлено" },
            new() { Id = 1, Name = "Очікує відправлення" }
        });

        _service = new DeliveryService(_orderRepository.Object, _waybillRepository.Object, _shipmentRepository.Object,
            _statusRepository.Object, _session, _timeProvider.Object);
    }

    private static CreateWaybillDto ValidWaybill() => new()
    {
        OrderId = 1,
        WaybillNumber = "  WB-1 ",
        PaymentType = DeliveryService.PaymentTypes[0],
        DeliveryMethod = DeliveryService.DeliveryMethods[1],
        City = " Київ ",
        DeliveryAddress = " Хрещатик, 1 ",
        Notes = "  "
    };

    private static List<DeliveryOrderDto> SampleRows() =>
    [
        new() { OrderId = 1, OrderNumber = "ORD-1", OrderDate = new DateTime(2026, 10, 1, 15, 0, 0), CustomerName = "Віктор", TotalAmount = 100, StatusId = 0 },
        new() { OrderId = 2, OrderNumber = "ORD-2", OrderDate = new DateTime(2026, 10, 5), CustomerName = "Анна", TotalAmount = 300, StatusId = 1, City = "Львів" },
        new() { OrderId = 3, OrderNumber = "ORD-3", OrderDate = new DateTime(2026, 10, 9), CustomerName = "Борис", TotalAmount = 200, StatusId = 2, WaybillNumber = "WB-3" }
    ];

    [Test]
    public void GetOrdersAsync_NotSignedIn_Throws()
    {
        _session.SignOut();

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.GetOrdersAsync());
        _orderRepository.Verify(r => r.GetAllWithDeliveryAsync(), Times.Never);
    }

    [Test]
    public void CreateWaybillAsync_UserWithoutManagerRole_ThrowsAndDoesNotSave()
    {
        _session.SignIn(new UserDto { Id = 10, Login = "customer", IsDeliveryManager = false });

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(ValidWaybill()));

        Assert.That(ex!.Message, Does.Contain("менеджеру доставки"));
        _waybillRepository.Verify(r => r.AddAsync(It.IsAny<Waybill>()), Times.Never);
    }

    [Test]
    public async Task GetStatusesAsync_AwaitingWaybillFirst_ThenStatusesById()
    {
        var result = await _service.GetStatusesAsync();

        Assert.That(result.Select(s => s.Id), Is.EqualTo(new[] { 0, 1, 2 }));
        Assert.That(result[0].Name, Is.EqualTo(DeliveryService.AwaitingWaybillStatusName));
    }

    [Test]
    public async Task GetOrdersAsync_MapsCustomerWaybillAndStatus()
    {
        var result = await _service.GetOrdersAsync();

        Assert.That(result, Has.Count.EqualTo(2));
        var withoutWaybill = result.Single(o => o.OrderId == 1);
        var withWaybill = result.Single(o => o.OrderId == 2);
        Assert.Multiple(() =>
        {
            Assert.That(withoutWaybill.CustomerName, Is.EqualTo("Іван Петренко"));
            Assert.That(withoutWaybill.StatusId, Is.EqualTo(DeliveryService.AwaitingWaybillStatusId));
            Assert.That(withoutWaybill.WaybillId, Is.Null);
            Assert.That(withWaybill.WaybillNumber, Is.EqualTo("WB-2"));
            Assert.That(withWaybill.ShipmentId, Is.EqualTo(30));
            Assert.That(withWaybill.StatusName, Is.EqualTo("Очікує відправлення"));
        });
    }

    [Test]
    public void ApplyFilter_DateRange_IncludesWholeBoundaryDays()
    {
        var filter = new DeliveryOrderFilterDto { DateFrom = new DateTime(2026, 10, 1), DateTo = new DateTime(2026, 10, 5) };

        var result = _service.ApplyFilter(SampleRows(), filter);

        Assert.That(result.Select(o => o.OrderId), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void ApplyFilter_DateFromAfterDateTo_Throws()
    {
        var filter = new DeliveryOrderFilterDto { DateFrom = new DateTime(2026, 10, 10), DateTo = new DateTime(2026, 10, 1) };

        Assert.Throws<BusinessRuleException>(() => _service.ApplyFilter(SampleRows(), filter));
    }

    [TestCase(0, 1)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    public void ApplyFilter_ByStatus_ReturnsOnlyThatStatus(int statusId, int expectedOrderId)
    {
        var result = _service.ApplyFilter(SampleRows(), new DeliveryOrderFilterDto { StatusId = statusId });

        Assert.That(result.Select(o => o.OrderId), Is.EqualTo(new[] { expectedOrderId }));
    }

    [TestCase("ord-2", 2)]
    [TestCase("борис", 3)]
    [TestCase("львів", 2)]
    [TestCase("wb-3", 3)]
    public void ApplyFilter_Search_IgnoresCaseAndSearchesSeveralFields(string text, int expectedOrderId)
    {
        var result = _service.ApplyFilter(SampleRows(), new DeliveryOrderFilterDto { SearchText = text });

        Assert.That(result.Select(o => o.OrderId), Is.EqualTo(new[] { expectedOrderId }));
    }

    [TestCase(OrderSortField.TotalAmount, false, new[] { 1, 3, 2 })]
    [TestCase(OrderSortField.TotalAmount, true, new[] { 2, 3, 1 })]
    [TestCase(OrderSortField.CustomerName, false, new[] { 2, 3, 1 })]
    [TestCase(OrderSortField.OrderDate, true, new[] { 3, 2, 1 })]
    [TestCase(OrderSortField.Status, false, new[] { 1, 2, 3 })]
    public void ApplyFilter_Sorting(OrderSortField field, bool descending, int[] expected)
    {
        var result = _service.ApplyFilter(SampleRows(), new DeliveryOrderFilterDto { SortBy = field, Descending = descending });

        Assert.That(result.Select(o => o.OrderId), Is.EqualTo(expected));
    }

    [Test]
    public async Task CreateWaybillAsync_Valid_SavesNormalizedWaybillAndPendingShipment()
    {
        Waybill? savedWaybill = null;
        Shipment? savedShipment = null;
        _waybillRepository.Setup(r => r.AddAsync(It.IsAny<Waybill>()))
            .Callback<Waybill>(w => { w.Id = 77; savedWaybill = w; })
            .Returns(Task.CompletedTask);
        _shipmentRepository.Setup(r => r.AddAsync(It.IsAny<Shipment>()))
            .Callback<Shipment>(s => savedShipment = s)
            .Returns(Task.CompletedTask);

        var id = await _service.CreateWaybillAsync(ValidWaybill());

        Assert.That(id, Is.EqualTo(77));
        Assert.Multiple(() =>
        {
            Assert.That(savedWaybill!.WaybillNumber, Is.EqualTo("WB-1"));
            Assert.That(savedWaybill.City, Is.EqualTo("Київ"));
            Assert.That(savedWaybill.DeliveryAddress, Is.EqualTo("Хрещатик, 1"));
            Assert.That(savedWaybill.CustomerId, Is.EqualTo(10));
            Assert.That(savedWaybill.CreatedAt, Is.EqualTo(Now.DateTime));
            Assert.That(savedShipment!.WaybillId, Is.EqualTo(77));
            Assert.That(savedShipment.ManagerId, Is.EqualTo(5));
            Assert.That(savedShipment.StatusId, Is.EqualTo(ShipmentStatus.PendingId));
            Assert.That(savedShipment.Notes, Is.Null);
        });
    }

    [Test]
    public void CreateWaybillAsync_EmptyFields_ReturnsAllErrorsAndDoesNotSave()
    {
        var dto = new CreateWaybillDto { OrderId = 1 };

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(dto));

        Assert.That(ex!.Errors, Has.Count.EqualTo(5));
        _waybillRepository.Verify(r => r.AddAsync(It.IsAny<Waybill>()), Times.Never);
        _shipmentRepository.Verify(r => r.AddAsync(It.IsAny<Shipment>()), Times.Never);
    }

    [Test]
    public void CreateWaybillAsync_OrderAlreadyHasWaybill_Throws()
    {
        var dto = ValidWaybill();
        dto.OrderId = 2;

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(dto));

        Assert.That(ex!.Message, Does.Contain("вже створена"));
    }

    [Test]
    public void CreateWaybillAsync_UnknownOrder_Throws()
    {
        var dto = ValidWaybill();
        dto.OrderId = 999;

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(dto));

        Assert.That(ex!.Message, Does.Contain("не знайдено"));
    }

    [Test]
    public void CreateWaybillAsync_DuplicateNumber_Throws()
    {
        _waybillRepository.Setup(r => r.ExistsByNumberAsync("WB-1")).ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(ValidWaybill()));

        Assert.That(ex!.Message, Does.Contain("вже існує"));
        _waybillRepository.Verify(r => r.AddAsync(It.IsAny<Waybill>()), Times.Never);
    }

    [Test]
    public void CreateWaybillAsync_UnknownPaymentType_Throws()
    {
        var dto = ValidWaybill();
        dto.PaymentType = "Біткоїн";

        var ex = Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(dto));

        Assert.That(ex!.Errors, Has.Some.Contains("спосіб оплати"));
    }

    [Test]
    public void CreateWaybillAsync_CityLongerThanLimit_Throws()
    {
        var dto = ValidWaybill();
        dto.City = new string('К', DeliveryService.CityMaxLength + 1);

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateWaybillAsync(dto));
    }

    [Test]
    public void CreateWaybillAsync_DoesNotChangeInputDto()
    {
        var dto = ValidWaybill();

        Assert.DoesNotThrowAsync(() => _service.CreateWaybillAsync(dto));
        Assert.That(dto.WaybillNumber, Is.EqualTo("  WB-1 "));
        Assert.That(dto.City, Is.EqualTo(" Київ "));
    }

    [Test]
    public async Task ConfirmDispatchAsync_PendingShipment_SetsShippedAndDate()
    {
        var shipment = new Shipment { Id = 30, StatusId = ShipmentStatus.PendingId, ManagerId = 1 };
        _shipmentRepository.Setup(r => r.GetByIdAsync(30)).ReturnsAsync(shipment);

        await _service.ConfirmDispatchAsync(30);

        Assert.Multiple(() =>
        {
            Assert.That(shipment.StatusId, Is.EqualTo(ShipmentStatus.ShippedId));
            Assert.That(shipment.DispatchedAt, Is.EqualTo(Now.DateTime));
            Assert.That(shipment.ManagerId, Is.EqualTo(5));
        });
        _shipmentRepository.Verify(r => r.UpdateAsync(shipment), Times.Once);
    }

    [Test]
    public void ConfirmDispatchAsync_AlreadyShipped_ThrowsAndDoesNotUpdate()
    {
        _shipmentRepository.Setup(r => r.GetByIdAsync(30))
            .ReturnsAsync(new Shipment { Id = 30, StatusId = ShipmentStatus.ShippedId });

        Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmDispatchAsync(30));
        _shipmentRepository.Verify(r => r.UpdateAsync(It.IsAny<Shipment>()), Times.Never);
    }

    [Test]
    public void ConfirmDispatchAsync_UnknownShipment_Throws()
    {
        Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmDispatchAsync(999));
    }
}
