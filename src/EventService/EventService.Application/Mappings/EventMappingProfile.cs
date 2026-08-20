using AutoMapper;
using EventOrchestrationService.Contracts.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application.Mappings;

public class EventMappingProfile : Profile
{
    public EventMappingProfile()
    {
        CreateMap<Event, EventContractDto>();
        CreateMap<CreateEventContractDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<UpdateEventContractDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}