using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Store.API.Middleware;
using Xunit;

namespace Store.API.Tests;

public class ETagMiddlewareTests
{
    [Theory]
    [InlineData(null, "W/\"12345\"", false)]
    [InlineData("", "W/\"12345\"", false)]
    [InlineData("   ", "W/\"12345\"", false)]
    [InlineData("*", "W/\"12345\"", true)]
    [InlineData("W/\"12345\"", "W/\"12345\"", true)]
    [InlineData("\"12345\"", "W/\"12345\"", true)]
    [InlineData("W/\"12345\"", "\"12345\"", true)]
    [InlineData("\"12345\"", "\"12345\"", true)]
    [InlineData("W/\"other\"", "W/\"12345\"", false)]
    [InlineData("\"foo\", W/\"bar\", \"12345\"", "W/\"12345\"", true)]
    [InlineData("\"foo\", \"bar\", W/\"12345\"", "W/\"12345\"", true)]
    [InlineData("\"foo\", \"bar\", \"baz\"", "W/\"12345\"", false)]
    [InlineData("\"foo\", *", "W/\"12345\"", true)]
    public void IsIfNoneMatchHit_EvaluatesCorrectly(string? ifNoneMatch, string? currentEtag, bool expected)
    {
        var result = ETagMiddleware.IsIfNoneMatchHit(ifNoneMatch, currentEtag);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task InvokeAsync_Get200_EmitsWeakEtagAndCacheControl()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/items";

        var responseContent = "{\"message\":\"hello world\"}";
        var responseBytes = Encoding.UTF8.GetBytes(responseContent);

        var middleware = new ETagMiddleware(
            next: async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                await ctx.Response.Body.WriteAsync(responseBytes);
            },
            logger: NullLogger<ETagMiddleware>.Instance);

        var outputStream = new MemoryStream();
        context.Response.Body = outputStream;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.True(context.Response.Headers.ContainsKey("ETag"));
        var etag = context.Response.Headers["ETag"].ToString();
        Assert.StartsWith("W/\"", etag);
        Assert.True(context.Response.Headers.ContainsKey("Cache-Control"));
        Assert.Contains("private, max-age=0, must-revalidate", context.Response.Headers["Cache-Control"].ToString());

        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var body = await reader.ReadToEndAsync();
        Assert.Equal(responseContent, body);
    }

    [Fact]
    public async Task InvokeAsync_MatchingIfNoneMatch_Returns304NotModifiedWithEmptyBody()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/items";

        var responseContent = "{\"data\":\"test payload\"}";
        var responseBytes = Encoding.UTF8.GetBytes(responseContent);

        // First pass: compute expected ETag
        string expectedEtag;
        {
            var testContext = new DefaultHttpContext();
            testContext.Request.Method = "GET";
            var tempStream = new MemoryStream();
            testContext.Response.Body = tempStream;
            var probeMiddleware = new ETagMiddleware(
                next: async ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status200OK;
                    await ctx.Response.Body.WriteAsync(responseBytes);
                },
                logger: NullLogger<ETagMiddleware>.Instance);
            await probeMiddleware.InvokeAsync(testContext);
            expectedEtag = testContext.Response.Headers["ETag"].ToString();
        }

        // Set matching header
        context.Request.Headers.IfNoneMatch = expectedEtag;
        var outputStream = new MemoryStream();
        context.Response.Body = outputStream;

        var middleware = new ETagMiddleware(
            next: async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                await ctx.Response.Body.WriteAsync(responseBytes);
            },
            logger: NullLogger<ETagMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status304NotModified, context.Response.StatusCode);
        Assert.Equal(0, context.Response.ContentLength);
        Assert.Equal(expectedEtag, context.Response.Headers["ETag"].ToString());
        Assert.Equal(0, outputStream.Length);
    }

    [Fact]
    public async Task InvokeAsync_WildcardIfNoneMatch_Returns304NotModified()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/customers";
        context.Request.Headers.IfNoneMatch = "*";

        var responseContent = "{\"customers\":[]}";
        var responseBytes = Encoding.UTF8.GetBytes(responseContent);
        var outputStream = new MemoryStream();
        context.Response.Body = outputStream;

        var middleware = new ETagMiddleware(
            next: async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                await ctx.Response.Body.WriteAsync(responseBytes);
            },
            logger: NullLogger<ETagMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status304NotModified, context.Response.StatusCode);
        Assert.Equal(0, outputStream.Length);
    }

    [Fact]
    public async Task InvokeAsync_PostRequest_BypassesEtagGeneration()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/v1/auth/login";

        var responseContent = "{\"token\":\"jwt-xyz\"}";
        var responseBytes = Encoding.UTF8.GetBytes(responseContent);
        var outputStream = new MemoryStream();
        context.Response.Body = outputStream;

        var middleware = new ETagMiddleware(
            next: async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                await ctx.Response.Body.WriteAsync(responseBytes);
            },
            logger: NullLogger<ETagMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("ETag"));
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var body = await reader.ReadToEndAsync();
        Assert.Equal(responseContent, body);
    }

    [Fact]
    public async Task InvokeAsync_CacheControlNoStore_SuppressesEtagAnd304()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/finance/sensitive";
        context.Request.Headers.IfNoneMatch = "*";

        var responseContent = "{\"sensitive\":\"data\"}";
        var responseBytes = Encoding.UTF8.GetBytes(responseContent);
        var outputStream = new MemoryStream();
        context.Response.Body = outputStream;

        var middleware = new ETagMiddleware(
            next: async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                ctx.Response.Headers.CacheControl = "no-store, no-cache";
                await ctx.Response.Body.WriteAsync(responseBytes);
            },
            logger: NullLogger<ETagMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("ETag"));
        Assert.Equal("no-store, no-cache", context.Response.Headers.CacheControl.ToString());
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var body = await reader.ReadToEndAsync();
        Assert.Equal(responseContent, body);
    }

    [Fact]
    public async Task InvokeAsync_PreservesDownstreamCustomEtag()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/reports/custom";
        context.Request.Headers.IfNoneMatch = "W/\"custom-controller-tag\"";

        var responseContent = "{\"report\":\"ready\"}";
        var responseBytes = Encoding.UTF8.GetBytes(responseContent);
        var outputStream = new MemoryStream();
        context.Response.Body = outputStream;

        var middleware = new ETagMiddleware(
            next: async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                ctx.Response.Headers.ETag = "W/\"custom-controller-tag\"";
                await ctx.Response.Body.WriteAsync(responseBytes);
            },
            logger: NullLogger<ETagMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status304NotModified, context.Response.StatusCode);
        Assert.Equal("W/\"custom-controller-tag\"", context.Response.Headers.ETag.ToString());
        Assert.Equal(0, outputStream.Length);
    }
}
