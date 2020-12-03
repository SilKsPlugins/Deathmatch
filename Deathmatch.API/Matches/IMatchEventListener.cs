using OpenMod.API.Eventing;
using System.Threading.Tasks;

namespace Deathmatch.API.Matches
{
    public interface IMatchEventListener<in TEvent> where TEvent : IEvent
    {
        Task HandleEventAsync(object sender, TEvent @event);
    }
}
