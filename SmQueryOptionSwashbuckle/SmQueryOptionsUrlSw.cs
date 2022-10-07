using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmQueryOptions;
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

namespace SmQueryOptionsSw;

[ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder))]
public class SmQueryOptionsUrlSw: SmQueryOptionsUrl
{
   
}

