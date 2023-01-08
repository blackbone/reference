#if ADDRESSABLES && !UNITASK
namespace References
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal static class AsyncOperationExtensions
    {
        internal static Task<T> ToUniTask<T>(this AsyncOperationHandle<T> op, IProgress<float> progress, CancellationToken cancellationToken = default)
        {
            return Awaiter();
            
            async Task<T> Awaiter()
            {
                while (!op.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(op.PercentComplete / 100);
                    await Task.Yield();
                }
                
                return op.Status == AsyncOperationStatus.Succeeded ? op.Result : default;
            }
        }
    }
}
#endif