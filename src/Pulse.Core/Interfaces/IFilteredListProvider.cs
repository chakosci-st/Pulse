using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Interfaces
{
  public  interface IFilteredListProvider<TEntity>
    {
        Task<IEnumerable<TEntity>> GetListAsync(string x = null);
    }
}
