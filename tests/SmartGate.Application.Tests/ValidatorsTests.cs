using FluentValidation.TestHelper;
using SmartGate.Application.Visits.Dto;
using SmartGate.Domain.Visits;

namespace SmartGate.Application.UnitTests;

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
            Status: null,
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
        var longKey = new string('K', 129);

        var req = new CreateVisitRequest(
            TruckLicensePlate: longPlate,
            Status: null,
            Driver: new DriverDto(new string('a', 129), new string('b', 129), "DFDS20245768"),
            Activities: [new ActivityDto(ActivityType.Delivery, new string('u', 33))],
            IdempotencyKey: longKey);

        var result = v.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.TruckLicensePlate);
        result.ShouldHaveValidationErrorFor("Driver.FirstName");
        result.ShouldHaveValidationErrorFor("Driver.LastName");
        result.ShouldHaveValidationErrorFor("Activities[0].UnitNumberRaw");
        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void UpdateVisitStatusRequestValidator_Validates()
    {
        var v = new UpdateVisitStatusRequestValidator();

        var ok = new UpdateVisitStatusRequest(Guid.NewGuid(), VisitStatus.OnSite);
        v.TestValidate(ok).ShouldNotHaveAnyValidationErrors();

        var badId = new UpdateVisitStatusRequest(Guid.Empty, VisitStatus.AtGate);
        v.TestValidate(badId).ShouldHaveValidationErrorFor(x => x.VisitId);
    }

    [Fact]
    public void CreateVisitRequestValidator_EnforceFirstStatus()
    {
        var v = new CreateVisitRequestValidator();
        var req = new CreateVisitRequest(
            TruckLicensePlate: "ABC123",
            Driver: new DriverDto("Luke", "Skywalker", "DFDS20245768"),
            Activities: [new ActivityDto(ActivityType.Delivery, "DFDS123456")],
            Status: "OnSite",
            IdempotencyKey: null);

        var result = v.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("New visits must have status 'PreRegistered'");
    }

}