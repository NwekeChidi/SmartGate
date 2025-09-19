using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartGate.Api.ErrorHandling;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits.Dto;

namespace SmartGate.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public sealed class VisitsController : ControllerBase
{
    private readonly IVisitService _svc;

    public VisitsController(IVisitService svc) => _svc = svc;

    [HttpPost("create")]
    [Authorize(Policy = "VisitsWrite")]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status201Created)]
    public async Task<IResult> Create([FromBody] CreateVisitRequest request, CancellationToken ct)
    {
        try
        {
            var res = await _svc.CreateVisitAsync(request, ct);
            return Results.Created($"/v1/visits/{res.Id}", res);
        }
        catch (Exception ex)
        {
            return ex.ToProblem(HttpContext);
        }
    }

    [HttpGet("/")]
    [Authorize(Policy = "VisitsRead")]
    [ProducesResponseType(typeof(PaginatedResult<VisitResponse>), StatusCodes.Status200OK)]
    public async Task<IResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        try
        {
            var res = await _svc.ListVisitsAsync(page, pageSize, ct);
            return Results.Ok(res);
        }
        catch (Exception ex)
        {
            return ex.ToProblem(HttpContext);
        }
    }

    [HttpPatch("status_update/{id:guid}")]
    [Authorize(Policy = "VisitsWrite")]
    [ProducesResponseType(typeof(VisitResponse), StatusCodes.Status200OK)]
    public async Task<IResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateVisitStatusRequest body, CancellationToken ct)
    {
        try
        {
            if (body.VisitId != id)
                return Results.BadRequest(new { message = "Route id and body.VisitId must match." });

            var res = await _svc.UpdateVisitStatusAsync(body, ct);
            return Results.Ok(res);
        }
        catch (Exception ex)
        {
            return ex.ToProblem(HttpContext);
        }
    }
}
