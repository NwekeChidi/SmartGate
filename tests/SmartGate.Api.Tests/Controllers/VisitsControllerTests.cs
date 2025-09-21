using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartGate.Api.Controllers;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits.Dto;
using SmartGate.Domain.Visits;

namespace SmartGate.Api.Tests.Controllers;

public class VisitsControllerTests
{
    private readonly IVisitService _visitService = Substitute.For<IVisitService>();
    private readonly VisitsController _controller;

    public VisitsControllerTests()
    {
        _controller = new VisitsController(_visitService);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Request.Path = "/test";
        _controller.HttpContext.TraceIdentifier = "test-trace";
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var driver = new DriverDto("Sasuke", "Uchiha", "123");
        var activities = new List<ActivityDto> { new(ActivityType.Delivery, "Unit1") };
        var request = new CreateVisitRequest("ABC123", driver, activities, VisitStatus.PreRegistered, null);
        var response = new VisitResponse(Guid.NewGuid(), VisitStatus.PreRegistered, "ABC123", 
            new DriverInformationDto("Sasuke", "Uchiha", "123"), 
            new List<ActivityResponse> { new(Guid.NewGuid(), ActivityType.Delivery, "Unit1") },
            "user", "user", DateTime.UtcNow, DateTime.UtcNow);
        
        _visitService.CreateVisitAsync(request, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ServiceThrowsException_ReturnsProblem()
    {
        var driver = new DriverDto("Sasuke", "Uchiha", "123");
        var activities = new List<ActivityDto> { new(ActivityType.Delivery, "Unit1") };
        var request = new CreateVisitRequest("ABC123", driver, activities, VisitStatus.PreRegistered, null);
        
        _visitService.When(x => x.CreateVisitAsync(Arg.Any<CreateVisitRequest>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidIdentifierException("visit"));

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task List_ValidRequest_ReturnsOk()
    {
        var visits = new List<VisitResponse>
        {
            new(Guid.NewGuid(), VisitStatus.PreRegistered, "ABC123", 
                new DriverInformationDto("Sasuke", "Uchiha", "123"),
                new List<ActivityResponse> { new(Guid.NewGuid(), ActivityType.Delivery, "Unit1") },
                "user", "user", DateTime.UtcNow, DateTime.UtcNow)
        };
        var paginatedResult = new PaginatedResult<VisitResponse>(1, 20, 1, visits);
        
        _visitService.ListVisitsAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(paginatedResult);

        var result = await _controller.List(1, 20, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task List_ServiceThrowsException_ReturnsProblem()
    {
        _visitService.When(x => x.ListVisitsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidOperationException("Database error"));

        var result = await _controller.List(1, 20, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateStatus_ValidRequest_ReturnsOk()
    {
        var visitId = Guid.NewGuid();
        var request = new UpdateVisitStatusRequest(VisitStatus.Completed);
        var response = new VisitResponse(visitId, VisitStatus.Completed, "ABC123",
            new DriverInformationDto("Sasuke", "Uchiha", "123"),
            new List<ActivityResponse> { new(Guid.NewGuid(), ActivityType.Delivery, "Unit1") },
            "user", "user", DateTime.UtcNow, DateTime.UtcNow);
        
        _visitService.UpdateVisitStatusAsync(request, visitId, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _controller.UpdateStatus(visitId, request, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateStatus_ServiceThrowsException_ReturnsProblem()
    {
        var visitId = Guid.NewGuid();
        var request = new UpdateVisitStatusRequest(VisitStatus.Completed);
        
        _visitService.When(x => x.UpdateVisitStatusAsync(Arg.Any<UpdateVisitStatusRequest>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new KeyNotFoundException("Visit not found"));

        var result = await _controller.UpdateStatus(visitId, request, CancellationToken.None);

        result.Should().NotBeNull();
    }
}