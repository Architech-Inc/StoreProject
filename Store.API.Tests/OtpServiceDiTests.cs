using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Store.DbServices.Extensions;
using Store.DbServices.Services;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class OtpServiceDiTests
{
    [Fact]
    public void OtpPepperOptions_ResolvesDirectlyAndViaIOptions()
    {
        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Server=localhost;Database=test;User=root;Password=pass;",
            ["Auth:OtpPepper"] = "DEV-ONLY-DO-NOT-DEPLOY-otp-pepper-at-least-32-bytes!!"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddScoped<IRealTimeNotificationService>(_ => Mock.Of<IRealTimeNotificationService>());
        services.AddStoreDbServices(configuration);

        var sp = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var options = sp.GetService<IOptions<OtpPepperOptions>>();
        Assert.NotNull(options);
        Assert.Equal("DEV-ONLY-DO-NOT-DEPLOY-otp-pepper-at-least-32-bytes!!", options.Value.OtpPepper);

        var direct = sp.GetService<OtpPepperOptions>();
        Assert.NotNull(direct);
        Assert.Equal("DEV-ONLY-DO-NOT-DEPLOY-otp-pepper-at-least-32-bytes!!", direct.OtpPepper);

        // Verify decoding
        var pepperBytes = direct.GetPepperBytes();
        Assert.True(pepperBytes.Length >= 32);
    }

    [Fact]
    public void OtpPepperOptions_ThrowsIfUnder32Bytes()
    {
        var opts = new OtpPepperOptions { OtpPepper = "short-key" };
        Assert.Throws<InvalidOperationException>(() => opts.GetPepperBytes());
    }
}
