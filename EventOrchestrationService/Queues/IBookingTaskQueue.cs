using EventOrchestrationService.Models;

namespace EventOrchestrationService.Queues;

public interface IBookingTaskQueue
{
    void Enqueue(Booking booking);
    bool TryDequeue(out Booking booking);
}