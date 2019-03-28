using IntegrationSolution.Common.Entities;
using Prism.Events;

namespace IntegrationSolution.Common.Events
{
    public class StatusUpdateEvent : PubSubEvent<Error>
    { }
}
