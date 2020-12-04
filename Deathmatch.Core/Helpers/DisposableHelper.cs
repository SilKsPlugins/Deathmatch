using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;

namespace Deathmatch.Core.Helpers
{
    public class DisposableHelper
    {
        public static async Task TryDispose(object obj)
        {
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (obj is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }

            if (obj is IUniTaskAsyncDisposable uniTaskAsyncDisposable)
            {
                await uniTaskAsyncDisposable.DisposeAsync().AsTask();
            }
        }
    }
}
