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

using System.ComponentModel;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that repeats each log event the specified number of times.
    /// </summary>
    /// <example>
    /// <p>This example causes each log message to be repeated 3 times.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/RepeatingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/RepeatingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("RepeatingWrapper", IsWrapper = true)]
    public class RepeatingTargetWrapper : WrapperTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the RepeatingTargetWrapper class.
        /// </summary>
        public RepeatingTargetWrapper()
        {
            this.RepeatCount = 3;
        }

        /// <summary>
        /// Initializes a new instance of the RepeatingTargetWrapper class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="repeatCount">The repeat count.</param>
        public RepeatingTargetWrapper(Target wrappedTarget, int repeatCount)
        {
            WrappedTarget = wrappedTarget;
            this.RepeatCount = repeatCount;
        }

        /// <summary>
        /// Gets or sets the number of times to repeat each log message.
        /// </summary>
        [DefaultValue(3)]
        public int RepeatCount { get; set; }

        /// <summary>
        /// Forwards the log message to the <see cref="WrapperTargetBase.WrappedTarget"/> by calling the <see cref="Target.Write(LogEventInfo)"/> method <see cref="RepeatCount"/> times.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            for (int i = 0; i < this.RepeatCount; ++i)
            {
                WrappedTarget.Write(logEvent);
            }
        }

        /// <summary>
        /// Forwards the array of log events to the <see cref="WrapperTargetBase.WrappedTarget"/> by calling the <see cref="Target.Write(LogEventInfo[])"/> method <see cref="RepeatCount"/> times.
        /// </summary>
        /// <param name="logEvents">The array of log events.</param>
        protected internal override void Write(LogEventInfo[] logEvents)
        {
            LogEventInfo[] newEvents = new LogEventInfo[logEvents.Length * this.RepeatCount];
            int pos = 0;

            for (int i = 0; i < logEvents.Length; ++i)
            {
                for (int j = 0; j < this.RepeatCount; ++j)
                {
                    newEvents[pos++] = logEvents[i];
                }
            }

            WrappedTarget.Write(newEvents);
        }
    }
}
