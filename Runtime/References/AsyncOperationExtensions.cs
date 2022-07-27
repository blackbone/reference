#if !UNITASK
namespace References
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    internal static class AsyncOperationExtensions
    {
        public static Task ToUniTask(this AsyncOperation op, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return Awaiter();
            
            async Task Awaiter()
            {
                while (!op.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(op.progress);
                    await Task.Yield();
                }
                
                progress?.Report(1);
            }
        }
    }
}
#endif