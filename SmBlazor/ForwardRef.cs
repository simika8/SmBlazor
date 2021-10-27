using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor
{
    public class ForwardRef
    {
        private ElementReference? _current;

        public ElementReference? Current
        {
            get => _current;
            set => Set(value);
        }
        public void Set(ElementReference? value)
        {
            _current = value;
        }
    }
}
