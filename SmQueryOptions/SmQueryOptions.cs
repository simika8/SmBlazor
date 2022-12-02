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

namespace SmQueryOptionsNs;


public class SmQueryOptions
{
    public int? Top { get; set; }
    public int? Skip { get; set; }
    public string? Search { get; set; }
    public HashSet<string>? Select { get; set; }

}
