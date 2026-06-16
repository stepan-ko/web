public class CameraWorker
{
    public Camera Camera { get; set; } = null!;

    public Task? Task { get; set; }

    public CancellationTokenSource? Cts { get; set; }

    public bool IsRunning =>
        Task != null &&
        !Task.IsCompleted &&
        !Task.IsFaulted &&
        !Task.IsCanceled;

    public string? LastError { get; set; }
}