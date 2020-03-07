using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public static class ResultsExtensions
    {
        //convert
        public static TotalResult<List<TTarget>> ToTotalResult<TSource, TTarget>(this RepositoryResult<TSource> result)
        {
            var data = result.Data.Cast<TTarget>().ToList();
            return new TotalResult<List<TTarget>>(data, result.TotalRows);
        }
        
    }
}
