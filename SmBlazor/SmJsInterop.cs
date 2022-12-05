using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SmBlazor
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class SmJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        public SmJsInterop(IJSRuntime jsRuntime)
        {
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/SmBlazor/smJsInterop.js").AsTask());
        }
        
        public async Task<IJSObjectReference> GetJsObjectReference()
        {
            return await _moduleTask.Value;
        }


        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_moduleTask.IsValueCreated)
                {
                    var module = await _moduleTask.Value;
                    await module.DisposeAsync();
                }
            } catch 
            {

            }
        }
    }
}