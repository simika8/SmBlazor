using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmQueryOptions;

public class SmQueryOptionsUrlBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        SmQueryOptionsUrl queryOptionsUrl = new SmQueryOptionsUrl();
        queryOptionsUrl.Top = Convert.ToInt32(bindingContext.ValueProvider.GetValue("Top").FirstOrDefault());
        queryOptionsUrl.Skip = Convert.ToInt32(bindingContext.ValueProvider.GetValue("Skip").FirstOrDefault());
        queryOptionsUrl.Search = bindingContext.ValueProvider.GetValue("Search").FirstOrDefault();
        queryOptionsUrl.Filter = bindingContext.ValueProvider.GetValue("Filter").FirstOrDefault();
        queryOptionsUrl.Orderby = bindingContext.ValueProvider.GetValue("Orderby").FirstOrDefault();
        queryOptionsUrl.Select = bindingContext.ValueProvider.GetValue("Select").FirstOrDefault();

        bindingContext.Result = ModelBindingResult.Success(queryOptionsUrl);
        return Task.FromResult(result: true);
    }
}
