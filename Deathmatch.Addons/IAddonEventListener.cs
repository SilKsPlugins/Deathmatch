using OpenMod.API.Eventing;
using System.Threading.Tasks;

namespace Deathmatch.Addons
{
    public interface IAddonEventListener<in TEvent> where TEvent : IEvent
    {
        Task HandleEventAsync(object sender, TEvent @event);
    }
}
