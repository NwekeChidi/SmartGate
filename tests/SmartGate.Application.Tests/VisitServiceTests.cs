using FluentAssertions;
using NSubstitute;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using SmartGate.Domain.Tests;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Application.Tests;

public class VisitServiceTests
{
    [Fact]
    public async Task CreateVisit_HappyPath_NormalizesAndMaps_AuditSetFromUser()
    {
        var repo = Substitute.For<IVisitRepository>();
        var user = TestHelpers.User("Yoda");
        var now = new DateTime(2025, 9, 18, 8, 30, 0, DateTimeKind.Utc);
        var clock = TestHelpers.FixedClock(now);
        var idem = TestHelpers.Idem(reserved: false);

        Visit? saved = null;
        repo.AddAsync(Arg.Do<Visit>(v => saved = v), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var service = TestHelpers.Service(repo, clock: clock, user: user, idem: idem);

        var req = new CreateVisitRequest(
            TruckLicensePlate: " abc 1234 ",
            Driver: new DriverDto("  Luke ", " Skywalker ", "DFDS-2024546375"),
            Activities:
            [
                new ActivityDto(ActivityType.Delivery, "dfds-123456 "),
                new ActivityDto(ActivityType.Collection, " dfds-654321")
            ],
            Status: VisitStatus.PreRegistered,
            IdempotencyKey: null
        );

        var response = await service.CreateVisitAsync(req);

        // assert
        response.TruckLicensePlate.Should().Be("ABC1234");
        response.Activities.Should().HaveCount(2);
        response.Activities.Select(a => a.UnitNumber).Should().BeEquivalentTo("DFDS123456", "DFDS654321");

        response.DriverInformation.FirstName.Should().Be("Luke");
        response.DriverInformation.LastName.Should().Be("Skywalker");

        response.CreatedAtUtc.Should().Be(now);
        response.UpdatedAtUtc.Should().Be(now);
        response.CreatedBy.Should().Be("Yoda");
        response.UpdatedBy.Should().Be("Yoda");

        saved.Should().NotBeNull();
        saved!.Truck.LicensePlateNormalized.Should().Be("ABC1234");
        saved!.Activities.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateVisit_WithIdempotencyKey_ThrowsDuplicateRequestException()
    {
        var repo = Substitute.For<IVisitRepository>();
        var visitId = Guid.NewGuid();

        var exisiting = new Visit(
            TestData.Truck("ABC123"),
            TestData.Driver("Senator", "Palpatine"),
            [TestData.Delivery("DFDS100")],
            id: visitId,
            nowUTC: DateTime.UtcNow,
            createdBy: "Yoda"
        );

        repo.GetByIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(exisiting);

        var idem = TestHelpers.Idem(reserved: false);
        var service = TestHelpers.Service(repo, idem: idem);

        var req = new CreateVisitRequest(
            TruckLicensePlate: " abc 12345 ",
            Driver: new DriverDto("  Luke ", " Skywalker ", "DFDS-199854635"),
            Activities:
            [
                new ActivityDto(ActivityType.Delivery, "dfds-123456 "),
                new ActivityDto(ActivityType.Collection, " dfds-654321")
            ],
            Status: VisitStatus.PreRegistered,
            IdempotencyKey: Guid.NewGuid()
        );

        await service.Invoking(s => s.CreateVisitAsync(req))
            .Should().ThrowAsync<DuplicateRequestException>();

        // assert
        await repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await repo.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task UpdateVisitStatus_ValidTransition_UpdatesAuditAndSaves()
    {
        var repo = Substitute.For<IVisitRepository>();
        var user = TestHelpers.User("Anakin");
        var createdTime = new DateTime(2025, 9, 18, 9, 0, 0, DateTimeKind.Utc);
        var updatedTime = new DateTime(2025, 9, 18, 10, 0, 0, DateTimeKind.Utc);

        var clock = Substitute.For<IClock>();

        var visit = new Visit(
            TestData.Truck("ABC123"),
            TestData.Driver("Senator", "Palpatine"),
            [TestData.Delivery("DFDS100")],
            nowUTC: createdTime,
            createdBy: user.Subject
        );

        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(visit);
        repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        clock.UtcNow.Returns(updatedTime);
        var service = TestHelpers.Service(repo, clock, user);

        var req = new UpdateVisitStatusRequest(VisitStatus.AtGate);

        var response = await service.UpdateVisitStatusAsync(req, Guid.NewGuid());

        // assert
        response.Status.Should().Be(VisitStatus.AtGate);
        response.CreatedAtUtc.Should().Be(createdTime);
        response.UpdatedAtUtc.Should().Be(updatedTime);
        response.UpdatedBy.Should().Be("Anakin");
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListVisits_PaginatesAndReturnsFullRecords()
    {
        var repo = Substitute.For<IVisitRepository>();
        var visit1 = new Visit(
            new Truck(" AB-12 CD "),
            new Driver("Gojo", "Satoru", "DFDS-20223547432"),
            [new Activity(ActivityType.Delivery, " dfds3009 ")],
            nowUTC: new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            createdBy: "Sukuna");

        var visit2 = new Visit(
            new Truck(" EF-34 GH "),
            new Driver("Luke", "Skywalker", "DFDS-20213547328"),
            [new Activity(ActivityType.Collection, " dfds3006 ")],
            nowUTC: new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            createdBy: "Darth");

        repo.ListAsync(Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
        .Returns(ci => Task.FromResult<IReadOnlyList<Visit>>([visit2, visit1]));

        var service = TestHelpers.Service(repo);

        var page = await service.ListVisitsAsync(page: 1, pageSize: 2);

        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.Count.Should().Be(2);

        page.Items.Should().HaveCount(2);

        var first = page.Items[0];
        first.TruckLicensePlate.Should().Be("EF34GH");
        first.DriverInformation.FirstName.Should().Be("Luke");
        first.DriverInformation.LastName.Should().Be("Skywalker");
        first.Activities.Should().ContainSingle(a => a.Type == ActivityType.Collection && a.UnitNumber == "DFDS3006");

        var second = page.Items[1];
        second.TruckLicensePlate.Should().Be("AB12CD");
        second.DriverInformation.FirstName.Should().Be("Gojo");
        second.DriverInformation.LastName.Should().Be("Satoru");
        second.Activities.Should().ContainSingle(a => a.Type == ActivityType.Delivery && a.UnitNumber == "DFDS3009");

    }

    [Fact]
    public async Task ListVisits_DefaultsParameters()
    {
        var repo = Substitute.For<IVisitRepository>();
        repo.ListAsync(Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult<IReadOnlyList<Visit>>([]));

        var service = TestHelpers.Service(repo);

        var page = await service.ListVisitsAsync(page: 0, pageSize: 0);

        page.Count.Should().Be(0);
        page.Items.Should().BeEmpty();
        page.Page.Should().Be(0);
        page.PageSize.Should().Be(0);

        await repo.Received(1)
                .ListAsync(
                    Arg.Is<PageRequest>(r => r.Page == 0 && r.PageSize == 0),
                    Arg.Any<CancellationToken>()
                );
    }

    [Fact]
    public async Task CreateVisit_IdempotencyKeyAlreadyReserved_ThrowsDuplicateRequestException()
    {
        var repo = Substitute.For<IVisitRepository>();
        var idem = Substitute.For<IIdempotencyStore>();
        var key = Guid.NewGuid();
        
        idem.TryReserveAsync(key, Arg.Any<CancellationToken>()).Returns(false);
        
        var service = TestHelpers.Service(repo, idem: idem);
        
        var req = new CreateVisitRequest(
            "ABC123",
            new DriverDto("Sasuke", "Uchiha", "DFDS-123"),
            [new ActivityDto(ActivityType.Delivery, "DFDS-456")],
            VisitStatus.PreRegistered,
            key
        );
        
        await service.Invoking(s => s.CreateVisitAsync(req))
            .Should().ThrowAsync<DuplicateRequestException>()
            .WithMessage($"A request with IdempotencyKey '{key}' already exists.");
    }

    [Fact]
    public async Task CreateVisit_ExistingDriver_ReusesDriver()
    {
        var repo = Substitute.For<IVisitRepository>();
        var drivers = Substitute.For<IDriverRepository>();
        var existingDriver = new Driver("Sasuke", "Uchiha", "DFDS-123");
        
        drivers.GetByIdAsync("DFDS-123", Arg.Any<CancellationToken>()).Returns(existingDriver);
        
        var service = TestHelpers.Service(repo, driver: drivers);
        
        var req = new CreateVisitRequest(
            "ABC123",
            new DriverDto("Sasuke", "Uchiha", "dfds-123"),
            [new ActivityDto(ActivityType.Delivery, "DFDS-456")],
            VisitStatus.PreRegistered,
            null
        );
        
        await service.CreateVisitAsync(req);
        
        await drivers.DidNotReceive().AddAsync(Arg.Any<Driver>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateVisit_WithValidIdempotencyKey_CompletesIdempotency()
    {
        var repo = Substitute.For<IVisitRepository>();
        var idem = Substitute.For<IIdempotencyStore>();
        var key = Guid.NewGuid();
        
        idem.TryReserveAsync(key, Arg.Any<CancellationToken>()).Returns(true);
        
        var service = TestHelpers.Service(repo, idem: idem);
        
        var req = new CreateVisitRequest(
            "ABC123",
            new DriverDto("Sasuke", "Uchiha", "DFDS-123"),
            [new ActivityDto(ActivityType.Delivery, "DFDS-456")],
            VisitStatus.PreRegistered,
            key
        );
        
        await service.CreateVisitAsync(req);
        
        await idem.Received(1).CompleteAsync(key, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListVisits_PageSizeOver200_ClampsTo200()
    {
        var repo = Substitute.For<IVisitRepository>();
        repo.ListAsync(Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Visit>>([]));;
        
        var service = TestHelpers.Service(repo);
        
        await service.ListVisitsAsync(1, 300);
        
        await repo.Received(1).ListAsync(
            Arg.Is<PageRequest>(r => r.PageSize == 200),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateVisitStatus_VisitNotFound_ThrowsKeyNotFoundException()
    {
        var repo = Substitute.For<IVisitRepository>();
        var visitId = Guid.NewGuid();
        
        repo.GetByIdAsync(visitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);
        
        var service = TestHelpers.Service(repo);
        var req = new UpdateVisitStatusRequest(VisitStatus.AtGate);
        
        await service.Invoking(s => s.UpdateVisitStatusAsync(req, visitId))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Visit not found.");
    }

    [Fact]
    public void ActivityResponse_Properties_AreAccessible()
    {
        var id = Guid.NewGuid();
        var response = new ActivityResponse(id, ActivityType.Delivery, "DFDS123");
        
        response.Id.Should().Be(id);
        response.Type.Should().Be(ActivityType.Delivery);
        response.UnitNumber.Should().Be("DFDS123");
    }
}