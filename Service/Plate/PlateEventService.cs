using System.Threading.Channels;

public class PlateEventService : IPlateEventService
{
    private readonly Channel<PlateEvent> _channel;

    public PlateEventService()
    {
        _channel = Channel.CreateUnbounded<PlateEvent>();
    }

    public void Raise(PlateEvent evt)
    {
        _channel.Writer.TryWrite(evt);
    }

    public IAsyncEnumerable<PlateEvent> ReadAllAsync(CancellationToken token)
        => _channel.Reader.ReadAllAsync(token);
}