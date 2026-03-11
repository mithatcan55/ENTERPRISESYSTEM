using Application.Exceptions;
using Host.Api.Integrations.Contracts;
using Host.Api.Integrations.Services;

namespace UnitTests;

public sealed class ExternalOutboxServiceNegativeTests
{
    [Fact]
    public async Task QueueMailAsync_Should_Throw_Validation_When_Required_Fields_Are_Missing()
    {
        var service = new ExternalOutboxService(null!, null!);

        var request = new QueueMailRequest
        {
            To = "",
            Subject = "",
            Body = ""
        };

        var exception = await Assert.ThrowsAsync<ValidationAppException>(() =>
            service.QueueMailAsync(request, CancellationToken.None));

        Assert.Equal("validation_error", exception.ErrorCode);
        Assert.True(exception.Errors?.ContainsKey("request"));
    }

    [Fact]
    public async Task QueueExcelReportAsync_Should_Throw_Validation_When_ReportName_Is_Missing()
    {
        var service = new ExternalOutboxService(null!, null!);

        var request = new QueueExcelReportRequest
        {
            ReportName = ""
        };

        var exception = await Assert.ThrowsAsync<ValidationAppException>(() =>
            service.QueueExcelReportAsync(request, CancellationToken.None));

        Assert.Equal("validation_error", exception.ErrorCode);
        Assert.True(exception.Errors?.ContainsKey("reportName"));
    }
}
