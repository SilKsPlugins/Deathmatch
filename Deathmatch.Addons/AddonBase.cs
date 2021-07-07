using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;

namespace Deathmatch.Addons
{
    public abstract class AddonBase : IAddon, IAsyncDisposable
    {
        public abstract string Title { get; }

        private bool _unloaded;

        public async ValueTask DisposeAsync()
        {
            await UnloadAsync();
        }

        public async UniTask LoadAsync()
        {
            await OnLoadAsync();
        }

        protected virtual UniTask OnLoadAsync() => UniTask.CompletedTask;

        public async UniTask UnloadAsync()
        {
            if (_unloaded)
            {
                return;
            }

            _unloaded = true;

            await OnUnloadAsync();
        }

        protected virtual UniTask OnUnloadAsync() => UniTask.CompletedTask;
    }
}
