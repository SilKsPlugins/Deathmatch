using Cysharp.Threading.Tasks;

namespace Deathmatch.Addons
{
    public interface IAddon
    {
        string Title { get; }

        UniTask LoadAsync();
        UniTask UnloadAsync();
    }
}
