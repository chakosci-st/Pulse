using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class PagedResult<T>
    {
        public int TotalRecords { get; set; } // Total number of rows (before paging)
        public List<T> Data { get; set; }    // Paged data
    }
}
