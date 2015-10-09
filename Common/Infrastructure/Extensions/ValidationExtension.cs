using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure.Extensions
{
    /// <summary>
    /// Extension method for common validation used for argument checking.
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        /// Method to verify if the object is null then throw exception.
        /// </summary>
        /// <typeparam name="T">Type of on which the method is invoked</typeparam>
        /// <param name="argument"></param>
        /// <param name="expression"></param>
        public static void CheckForNotNull<T>(this object source, Expression<Func<T>> argumentNameExpression = null, string message = "")
        {
            if (source == null)
            {
                string argumentName = string.Empty;

                if (argumentNameExpression != null)
                {
                    argumentName = NameOf(argumentNameExpression);
                }

                throw new ArgumentNullException(argumentName, "Parameter can not be null.");
            }
        }

        //public static void CheckForNotNull(this object source)
        //{
        //    source.CheckForNotNull();
        //}

        public static string NameOf<T>(Expression<Func<T>> action) 
        {
            var expression = (MemberExpression)action.Body;
            string name = expression.Member.Name;

            return name;
        }
    }
}
