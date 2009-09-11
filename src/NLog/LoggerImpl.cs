// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Diagnostics;
using System.Globalization;
using NLog.Config;
using NLog.Filters;
using NLog.Internal;
using NLog.Targets;

namespace NLog
{
    /// <summary>
    /// Implementation of logging engine.
    /// </summary>
    internal static class LoggerImpl
    {
        private const int StackTraceSkipMethods = 0;

        internal static void Write(Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory)
        {
            if (targets == null)
            {
                return;
            }

#if !NET_CF            
            StackTraceUsage stu = targets.GetStackTraceUsage();

            StackTrace stackTrace;
            if (stu != StackTraceUsage.None && !logEvent.HasStackTrace)
            {
                int firstUserFrame = 0;
                stackTrace = new StackTrace(StackTraceSkipMethods, stu == StackTraceUsage.WithSource);

                for (int i = 0; i < stackTrace.FrameCount; ++i)
                {
                    System.Reflection.MethodBase mb = stackTrace.GetFrame(i).GetMethod();

                    if (mb.DeclaringType == loggerType)
                    {
                        firstUserFrame = i + 1;
                    }
                    else
                    {
                        if (firstUserFrame != 0)
                        {
                            break;
                        }
                    }
                }

                logEvent.SetStackTrace(stackTrace, firstUserFrame);
            }
#endif 
            for (TargetWithFilterChain awf = targets; awf != null; awf = awf.NextInChain)
            {
                Target app = awf.Target;
                FilterResult result = FilterResult.Neutral;

                try
                {
                    foreach (Filter f in awf.FilterChain)
                    {
                        result = f.Check(logEvent);
                        if (result != FilterResult.Neutral)
                        {
                            break;
                        }
                    }

                    if ((result == FilterResult.Ignore) || (result == FilterResult.IgnoreFinal))
                    {
                        if (InternalLogger.IsDebugEnabled)
                        {
                            InternalLogger.Debug(CultureInfo.InvariantCulture, "{0}.{1} Rejecting message because of a filter.", logEvent.LoggerName, logEvent.Level);
                        }
                        
                        if (result == FilterResult.IgnoreFinal)
                        {
                            return;
                        }

                        continue;
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(CultureInfo.CurrentCulture, "FilterChain exception: {0}", ex);
                    if (factory.ThrowExceptions)
                    {
                        throw;
                    }

                    continue;
                }

                try
                {
                    app.Write(logEvent);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(CultureInfo.CurrentCulture, "Target exception: {0}", ex);
                    if (factory.ThrowExceptions)
                    {
                        throw;
                    }

                    continue;
                }

                if (result == FilterResult.LogFinal)
                {
                    return;
                }
            }
        }
    }
}
