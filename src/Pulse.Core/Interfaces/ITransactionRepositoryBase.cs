using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Interfaces
{
    public interface ITransactionRepositoryBase
    {
        // Transaction management
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
