using Microsoft.Data.SqlClient;
using Transformalize.Configuration;

namespace Transformalize.Providers.SqlServer {
   public class ClusteredKeyOrderHint : IOrderHint {
      public void Set(SqlBulkCopy bulkCopy, Field[] fields) {
         // version 1.x of Microsoft.Data.SqlClient doesn't support order hints
      }
   }
}
