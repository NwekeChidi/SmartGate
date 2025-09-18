using System.Collections.ObjectModel;
using FluentAssertions;
using NSubstitute;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Tests;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using SmartGate.Domain.Tests;
using SmartGate.Domain.Visits;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Application.UnitTests;

public class VisitServiceTests
{
    [Fact]
    public async Task CreateVisit_HappyPath_NormalizesAndMaps_AuditSetFromUser()
    {
        var repo = Substitute.For<IVisitRepository>();
        var user = TestHelpers.User("Yoda");
        var now = new DateTime(2025, 9, 18, 8, 30, 0, DateTimeKind.Utc);
        var clock = TestHelpers.FixedClock(now);
        var idem = TestHelpers.Idem(exists: false);

        Visit? saved = null;
        repo.AddAsync(Arg.Do<Visit>(v => saved = v), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var service = TestHelpers.Service(repo, clock: clock, user: user, idem: idem);

        var req = new CreateVisitRequest(
            TruckLicensePlate: " abc 1234 ",
            Driver: new DriverDto("  Luke ", " Skywalker "),
            Activities:
            [
                new ActivityDto(ActivityType.Delivery, "dfds-123456 "),
                new ActivityDto(ActivityType.Collection, " dfds-654321")
            ],
            Status: null,
            IdempotencyKey: null
        );

        var response = await service.CreateVisitAsync(req);

        // assert
        response.TruckLicensePlate.Should().Be("ABC1234");
        response.Activities.Should().HaveCount(2);
        response.Activities.Select(a => a.UnitNumber).Should().BeEquivalentTo("DFDS123456", "DFDS654321");

        response.DriverInformation.FirstName.Should().Be("Luke");
        response.DriverInformation.LastName.Should().Be("Skywalker");
        Guid.TryParse(response.DriverInformation.Id, out _).Should().BeTrue();

        response.CreatedAtUtc.Should().Be(now);
        response.UpdatedAtUtc.Should().Be(now);
        response.CreatedBy.Should().Be("Yoda");
        response.UpdatedBy.Should().Be("Yoda");

        saved.Should().NotBeNull();
        saved!.Truck.LicensePlateNormalized.Should().Be("ABC1234");
        saved!.Activities.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateVisit_WithIdempotencyKey_ReturnsExistingWithoutCreating()
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

        var idem = TestHelpers.Idem(exists: true, id: visitId);
        var service = TestHelpers.Service(repo, idem: idem);

        var req = new CreateVisitRequest(
            TruckLicensePlate: " abc 12345 ",
            Driver: new DriverDto("  Luke ", " Skywalker "),
            Activities:
            [
                new ActivityDto(ActivityType.Delivery, "dfds-123456 "),
                new ActivityDto(ActivityType.Collection, " dfds-654321")
            ],
            Status: null,
            IdempotencyKey: "idem-key-1"
        );

        var response = await service.CreateVisitAsync(req);

        // assert
        await repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await repo.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        response.Id.Should().Be(visitId);
        response.TruckLicensePlate.Should().Be("ABC123");
        response.DriverInformation.FirstName.Should().Be("Senator");
        response.DriverInformation.LastName.Should().Be("Palpatine");
        response.Activities.Should().HaveCount(1);
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

        clock.UTCNow.Returns(updatedTime);
        var service = TestHelpers.Service(repo, clock, user);

        var req = new UpdateVisitStatusRequest(visit.Id, VisitStatus.AtGate);

        var response = await service.UpdateVisitStatusAsync(req);

        // assert
        response.Status.Should().Be(VisitStatus.AtGate);
        response.CreatedAtUtc.Should().Be(createdTime);
        response.UpdatedAtUtc.Should().Be(updatedTime);
        response.UpdatedBy.Should().Be("Anakin");
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListVisits_PaginatesAndReturnsItems()
    {
        var repo = Substitute.For<IVisitRepository>();
        var items = new ReadOnlyCollection<VisitListItem>(
        [
            new VisitListItem(Guid.NewGuid(), VisitStatus.PreRegistered, "AB12CD", DateTime.UtcNow),
            new VisitListItem(Guid.NewGuid(), VisitStatus.AtGate, "EF34GH", DateTime.UtcNow),
            new VisitListItem(Guid.NewGuid(), VisitStatus.OnSite, "AB12FD", DateTime.UtcNow),
            new VisitListItem(Guid.NewGuid(), VisitStatus.AtGate, "GF34GH", DateTime.UtcNow)
        ]);

        repo.ListAsync(Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var service = TestHelpers.Service(repo);

        var result = await service.ListVisitsAsync(page: 2, pageSize: 2);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Count.Should().Be(4);
        result.Items.Should().BeEquivalentTo(items);
    }
}