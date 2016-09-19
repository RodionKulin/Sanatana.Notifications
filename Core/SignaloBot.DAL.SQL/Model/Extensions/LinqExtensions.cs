using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    public static class LinqExtensions
    {
        public static Expression<Func<T, bool>> Or<T>(
           this Expression<Func<T, bool>> one, Expression<Func<T, bool>> another)
        {
            var candidateExpr = Expression.Parameter(typeof(T), "candidate");
            var parameterReplacer = new ParameterReplacer(candidateExpr);

            var left = parameterReplacer.Replace(one.Body);
            var right = parameterReplacer.Replace(another.Body);
            var body = Expression.Or(left, right);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }

        public static Expression<Func<T, bool>> Or<T>(
            this IEnumerable<Expression<Func<T, bool>>> list)
        {
            Expression<Func<T, bool>> one = list.FirstOrDefault();
            IEnumerable<Expression<Func<T, bool>>> others = list.Skip(1);

            var candidateExpr = Expression.Parameter(typeof(T), "candidate");
            var parameterReplacer = new ParameterReplacer(candidateExpr);

            foreach (Expression<Func<T, bool>> another in others)
            {
                var left = parameterReplacer.Replace(one.Body);
                var right = parameterReplacer.Replace(another.Body);
                var body = Expression.Or(left, right);
                one = Expression.Lambda<Func<T, bool>>(body, candidateExpr);
            }

            return one;
        }

        public static Expression<Func<SignalDispatchBase<Guid>, bool>> ToExpression(
            Expression<Func<SignalDispatchBase<Guid>, bool>> expression)
        {
            return expression;
        }
    }
}
