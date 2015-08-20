#region File Information/History
// <copyright file="CancellationTokens.cs" project="WCFLoad" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.Threading;

namespace WCFLoad.Tokens
{
    /// <summary>
    /// Cancellation token class to cancel a long running the performance run in between
    /// </summary>
    public static class CancellationTokens
    {
        private static CancellationTokenSource _performanceRunTokenSource;
        private static CancellationToken _performanceRunToken;
        
        #region public properties
        
        public static CancellationTokenSource PerformanceRunTokenSource
        {
            get { return _performanceRunTokenSource; }
            set
            {
                _performanceRunTokenSource = value;
                if (_performanceRunTokenSource != null)
                    PerformanceRunToken = _performanceRunTokenSource.Token;
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public static CancellationToken PerformanceRunToken
        {
            get { return _performanceRunToken; }
            set { _performanceRunToken = value; }
        }

        #endregion

        #region public methods
        public static void ResetPerformanceCancellationToken()
        {
            if (PerformanceRunTokenSource != null)
            {
                PerformanceRunTokenSource.Dispose();
                PerformanceRunTokenSource = null;
            }
        }
        public static CancellationToken GetPerformanceCancellationToken()
        {
            if (PerformanceRunTokenSource == null)
                PerformanceRunTokenSource = new CancellationTokenSource();
            return PerformanceRunToken;
        }

        public static void CancelPerformanceCancellationTokenSource()
        {
            PerformanceRunTokenSource.Cancel();
        }
        #endregion
    }
}
