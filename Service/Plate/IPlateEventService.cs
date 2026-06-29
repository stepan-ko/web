public interface IPlateEventService
{
    void Raise(PlateEvent evt);
    IAsyncEnumerable<PlateEvent> ReadAllAsync(CancellationToken token);
}