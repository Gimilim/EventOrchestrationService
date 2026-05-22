using System.Collections.Concurrent;
using EventOrchestrationService.Models;

namespace EventOrchestrationService.Queues;

public class BookingTaskQueue : IBookingTaskQueue
{
    private readonly ConcurrentQueue<Booking> _queue = new();

    public void Enqueue(Booking booking)
    {
        _queue.Enqueue(booking);
    }

    public bool TryDequeue(out Booking booking)
    {
        return _queue.TryDequeue(out booking);
    }
}