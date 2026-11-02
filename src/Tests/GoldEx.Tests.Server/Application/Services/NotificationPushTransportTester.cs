using System.Net;
using System.Security.Cryptography;
using System.Text;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Application.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public sealed class NotificationPushTransportTester
{
    [TestCase(201, "Accepted")]
    [TestCase(410, "Gone")]
    [TestCase(429, "Retry")]
    [TestCase(503, "Retry")]
    [TestCase(401, "Rejected")]
    public async Task ActualLibraryEncryptsAndSignsUsingConfiguredKeys_AndClassifiesProviderResponses(int status, string expected)
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AGENTS.md"))) root = root.Parent;
        var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(root!.FullName, "src/App/Server/GoldEx.Server/appsettings.json")).Build();
        using var capture = new CaptureHandler((HttpStatusCode)status);
        var factory = new Mock<IHttpClientFactory>(); factory.Setup(x => x.CreateClient("GoldExWebPush")).Returns(() => new HttpClient(capture, disposeHandler: false));
        var sender = new WebPushSender(configuration, factory.Object);
        using var curve = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var key = curve.ExportParameters(false);
        string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var result = await sender.SendAsync("https://fcm.googleapis.com/test", Encode([4, ..key.Q.X!, ..key.Q.Y!]), Encode(RandomNumberGenerator.GetBytes(16)),
            new WebPushMessage("GoldEx", "test-notification-body", "/", "test"), default);
        Assert.That(result.Outcome, Is.EqualTo(expected));
        Assert.That(capture.Signed, Is.True);
        Assert.That(capture.Encoding, Is.Not.Empty);
        Assert.That(capture.Payload, Is.Not.Empty);
        Assert.That(Encoding.UTF8.GetString(capture.Payload), Does.Not.Contain("test-notification-body"));
    }
    private sealed class CaptureHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public bool Signed { get; private set; }
        public string Encoding { get; private set; } = "";
        public byte[] Payload { get; private set; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Signed = request.Headers.Contains("Authorization");
            Encoding = string.Join(',', request.Content!.Headers.ContentEncoding);
            Payload = await request.Content.ReadAsByteArrayAsync(ct);
            return new(status) { Content = new StringContent("") };
        }
    }
}
