using SmQueryOptionsNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor;

public interface IDataSource
{
    public Task<List<dynamic?>> GetRows(SmQueryOptions qo);
}
