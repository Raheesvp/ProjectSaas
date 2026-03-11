using DocumentService.Application.Interfaces;

namespace DocumentService.Infrastructure.Stubs;

public sealed class StubStorageService : IStorageService
{
    public Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default)
        => Task.FromResult(path);

    public Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
        => Task.FromResult<Stream>(new MemoryStream());

    public Task DeleteAsync(string path, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
        => Task.FromResult(false);

    public string GetPublicUrl(string path)
        => $"http://localhost:9000/{path}";
}
