using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace SmBlazor;

public partial class Modal
{
    private DotNetObjectReference<Modal> _this;
    private ElementReference _element;
    // Content of the dialog
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public EventCallback<string> Close { get; set; }

    //private Lazy<Task<IJSObjectReference>> moduleTask;
    private IJSObjectReference? _JsObjectReferenceReuse;
    private async Task<IJSObjectReference> GetJsObjectReference(IJSRuntime jsRuntime)
    {
        if (_JsObjectReferenceReuse is null)
        {
            Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/SmBlazor/smJsInterop.js").AsTask());
            _JsObjectReferenceReuse = await moduleTask.Value;
        }
        return _JsObjectReferenceReuse;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var JsObjectReference = await GetJsObjectReference(JsRuntime);
        if (firstRender)
        {
        }
        if (JsObjectReference is null)
            return;

        if (firstRender)
        {
            //IJSObjectReference? module = await moduleTask.Value;
            await JsObjectReference.InvokeVoidAsync("blazorInitializeModal", _element, _this);
        }

        if (Open)
        {
            //var module = await moduleTask.Value;
            await JsObjectReference.InvokeVoidAsync("blazorOpenModal", _element);
        }
        else
        {
            //var module = await moduleTask.Value;
            await JsObjectReference.InvokeVoidAsync("blazorCloseModal", _element);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public async Task OnClose(string returnValue)
    {
        if (Open == true)
        {
            Open = false;
            await OpenChanged.InvokeAsync(Open);
        }

        await Close.InvokeAsync(returnValue);
    }

    public void Open2()
    {
        Open = true;
    }

    public void Close2()
    {
        Open = false;
    }
}
