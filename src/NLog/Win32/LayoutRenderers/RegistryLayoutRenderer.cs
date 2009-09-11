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

#if !NET_CF_1_0 && !SILVERLIGHT

using System;
using System.Text;
using Microsoft.Win32;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Win32.LayoutRenderers
{
    /// <summary>
    /// A value from the Registry.
    /// </summary>
    [LayoutRenderer("registry")]
    public class RegistryLayoutRenderer : LayoutRenderer
    {
        private string key;
        private RegistryKey rootKey = Registry.LocalMachine;
        private string subKey;

        /// <summary>
        /// Gets or sets the registry value name.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the value to be output when the specified registry key or value is not found.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the registry key.
        /// </summary>
        /// <remarks>
        /// Must have one of the forms:
        /// <ul>
        /// <li>HKLM\Key\Full\Name</li>
        /// <li>HKEY_LOCAL_MACHINE\Key\Full\Name</li>
        /// <li>HKCU\Key\Full\Name</li>
        /// <li>HKEY_CURRENT_USER\Key\Full\Name</li>
        /// </ul>
        /// </remarks>
        [RequiredParameter]
        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
                int pos = this.key.IndexOfAny(new char[] { '\\', '/' });

                if (pos >= 0)
                {
                    string root = this.key.Substring(0, pos);
                    switch (root.ToUpper())
                    {
                        case "HKEY_LOCAL_MACHINE":
                        case "HKLM":
                            this.rootKey = Registry.LocalMachine;
                            break;

                        case "HKEY_CURRENT_USER":
                        case "HKCU":
                            this.rootKey = Registry.CurrentUser;
                            break;

                        default:
                            throw new ArgumentException("Key name is invalid. Root hive not recognized.");
                    }

                    this.subKey = this.key.Substring(pos + 1).Replace('/', '\\');
                }
                else
                {
                    throw new ArgumentException("Key name is invalid");
                }
            }
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// This function always returns 32.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 32;
        }

        /// <summary>
        /// Reads the specified registry key and value and appends it to
        /// the passed <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event. Ignored.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            using (RegistryKey key = this.rootKey.OpenSubKey(this.subKey))
            {
                builder.Append(key.GetValue(this.Value, this.DefaultValue));
            }
        }
    }
}

#endif
