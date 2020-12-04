namespace Deathmatch.Addons
{
    public interface IAddon
    {
        string Title { get; }

        void Load();
        void Unload();
    }
}
