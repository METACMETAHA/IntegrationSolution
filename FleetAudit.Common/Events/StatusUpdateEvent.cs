using FleetAudit.Common.Entities;
using Prism.Events;

namespace FleetAudit.Common.Events
{
    public class StatusUpdateEvent : PubSubEvent<Error>
    { }
}
