using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Store.DbServices.Services;
using Store.Models.Configuration;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class SmtpEmailSenderTests
{
    private readonly Mock<ILogger<SmtpEmailSender>> _senderLoggerMock;
    private readonly Mock<ILogger<NotificationService>> _notificationLoggerMock;
    private readonly Mock<ICommunicationLogService> _commLogMock;

    public SmtpEmailSenderTests()
    {
        _senderLoggerMock = new Mock<ILogger<SmtpEmailSender>>();
        _notificationLoggerMock = new Mock<ILogger<NotificationService>>();
        _commLogMock = new Mock<ICommunicationLogService>();
    }

    [Fact]
    public async Task SendEmailAsync_WhenDisabled_SimulatesDispatchAndReturnsTrue()
    {
        var options = Options.Create(new SmtpOptions
        {
            IsEnabled = false,
            Host = ""
        });

        var sender = new SmtpEmailSender(options, _senderLoggerMock.Object);

        var result = await sender.SendEmailAsync(
            toEmail: "customer@example.com",
            subject: "Your Order Confirmation",
            body: "Thank you for your order.",
            ct: CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task SendEmailAsync_WhenRecipientEmpty_ThrowsArgumentException()
    {
        var options = Options.Create(new SmtpOptions
        {
            IsEnabled = false
        });

        var sender = new SmtpEmailSender(options, _senderLoggerMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sender.SendEmailAsync(
                toEmail: "",
                subject: "Test",
                body: "Body",
                ct: CancellationToken.None));
    }

    [Fact]
    public async Task NotificationService_SendEmailAsync_InvokesSmtpEmailSenderAndLogsSuccess()
    {
        var smtpMock = new Mock<ISmtpEmailSender>();
        smtpMock
            .Setup(s => s.SendEmailAsync(
                "customer@example.com",
                "Password Reset OTP",
                "Your OTP is 123456",
                null,
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        CommunicationLog? recordedLog = null;
        _commLogMock
            .Setup(c => c.LogCommunicationAsync(It.IsAny<CommunicationLog>(), It.IsAny<CancellationToken>()))
            .Callback<CommunicationLog, CancellationToken>((log, _) => recordedLog = log)
            .Returns(Task.CompletedTask);

        var notificationService = new NotificationService(
            _commLogMock.Object,
            _notificationLoggerMock.Object,
            smtpMock.Object);

        var success = await notificationService.SendEmailAsync(
            "customer@example.com",
            "Password Reset OTP",
            "Your OTP is 123456",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(success);
        smtpMock.Verify(s => s.SendEmailAsync(
            "customer@example.com",
            "Password Reset OTP",
            "Your OTP is 123456",
            null,
            null,
            false,
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(recordedLog);
        Assert.Equal(CommunicationStatus.Sent, recordedLog.Status);
        Assert.Equal("customer@example.com", recordedLog.Recipient);
    }
}
