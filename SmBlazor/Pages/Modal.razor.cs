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

    public void ShowModal()
    {
        Open = true;
    }

    public void Close()
    {
        Open = false;
    }
    
}
