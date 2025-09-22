using FluentValidation.TestHelper;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Validators;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.Tests.Visits.Validators;

public class ValidatorsTests
{
    [Fact]
    public void CreateVisitRequestValidator_CatchesMissingFields()
    {
        var v = new CreateVisitRequestValidator();
        var req = new CreateVisitRequest(
            TruckLicensePlate: "",
            Driver: new DriverDto("", "", ""),
            Activities: [],
            Status: VisitStatus.PreRegistered,
            IdempotencyKey: null);

        var result = v.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.TruckLicensePlate);
        result.ShouldHaveValidationErrorFor("Driver.FirstName");
        result.ShouldHaveValidationErrorFor("Driver.LastName");
        result.ShouldHaveValidationErrorFor(x => x.Activities);
    }

    [Fact]
    public void CreateVisitRequestValidator_EnforcesLengths()
    {
        var v = new CreateVisitRequestValidator();
        var longPlate = new string('P', 33);

        var req = new CreateVisitRequest(
            TruckLicensePlate: longPlate,
            Status: VisitStatus.PreRegistered,
            Driver: new DriverDto(new string('a', 129), new string('b', 129), "DFDS-202457"),
            Activities: [new ActivityDto(ActivityType.Delivery, new string('d', 33))],
            IdempotencyKey: Guid.Empty);

        var result = v.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.TruckLicensePlate);
        result.ShouldHaveValidationErrorFor("Driver.FirstName");
        result.ShouldHaveValidationErrorFor("Driver.LastName");
        result.ShouldHaveValidationErrorFor("Activities[0].UnitNumber");
        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void CreateVisitRequestValidator_EnforceFirstStatus()
    {
        var v = new CreateVisitRequestValidator();
        var req = new CreateVisitRequest(
            TruckLicensePlate: "ABC123",
            Driver: new DriverDto("Luke", "Skywalker", "DFDS-202458"),
            Activities: [new ActivityDto(ActivityType.Delivery, "DFDS-123456")],
            Status: VisitStatus.OnSite,
            IdempotencyKey: null);

        var result = v.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("New visits must have status 'PreRegistered'");
    }

}