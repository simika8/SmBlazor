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
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public SmJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/SmBlazor/smJsInterop.js").AsTask());
        }
        public async ValueTask<bool> ScrollToElement(ElementReference? element)
        {
            if (element == null)
                return false;
            var module = await moduleTask.Value;
            try
            {
                return await module.InvokeAsync<bool>("scrollToElement", element);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public async ValueTask<int> ClientHeight(ElementReference? element)
        {
            if (element == null)
                return 0;
            var module = await moduleTask.Value;
            try
            {
                var res = await module.InvokeAsync<int>("clientHeight", element);
                return res;
            }
            catch
            {
                return 0;
            }
        }
        public async ValueTask<bool> FocusToElement(ElementReference? element)
        {
            if (element == null)
                return false;
            var module = await moduleTask.Value;
            return await module.InvokeAsync<bool>("focusToElement", element);
        }
        public async ValueTask<bool> SetValue(ElementReference? element, string text)
        {
            if (element == null)
                return false;
            var module = await moduleTask.Value;
            return await module.InvokeAsync<bool>("setValue", element, text);
        }

        public async ValueTask<bool> SubscribeToChange(DotNetObjectReference<Input> inputComponentRef, ElementReference? InputElementRef)
        {
            if (InputElementRef == null)
                return false;
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("subscribeToChange", new object[] { inputComponentRef, InputElementRef });
            return true;
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}