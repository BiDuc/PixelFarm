﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MatterHackers.SerialPortCommunication
{
    public class FoundStringEventArgs : EventArgs
    {
        public bool CallbackWasCalled { get; set; }
        bool sendToDelegateFunctions = true;
        string lineToCheck;

        public FoundStringEventArgs(string lineReceived)
        {
            this.lineToCheck = lineReceived.Trim();
        }

        public string LineToCheck { get { return lineToCheck; } }

        public bool SendToDelegateFunctions
        {
            get
            {
                return sendToDelegateFunctions;
            }
            set
            {
                sendToDelegateFunctions = value;
            }
        }
    }

    public class FoundStringCallBacks
    {
        public delegate void FoundStringEventHandler(object sender, EventArgs foundStringEventArgs);

        public Dictionary<string, FoundStringEventHandler> dictionaryOfCallBacks = new Dictionary<string, FoundStringEventHandler>();

        public void AddCallBackToKey(string key, FoundStringEventHandler value)
        {
            if (dictionaryOfCallBacks.ContainsKey(key))
            {
                dictionaryOfCallBacks[key] += value;
            }
            else
            {
                dictionaryOfCallBacks.Add(key, value);
            }
        }

        public void RemoveCallBackFromKey(string key, FoundStringEventHandler value)
        {
            if (dictionaryOfCallBacks.ContainsKey(key))
            {
                if (dictionaryOfCallBacks[key] == null)
                {
                    throw new Exception();
                }
                dictionaryOfCallBacks[key] -= value;
                if (dictionaryOfCallBacks[key] == null)
                {
                    dictionaryOfCallBacks.Remove(key);
                }
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public class FoundStringStartsWithCallbacks : FoundStringCallBacks
    {

        public void CheckForKeys(EventArgs e)
        {
            foreach (KeyValuePair<string, FoundStringEventHandler> pair in this.dictionaryOfCallBacks)
            {
                FoundStringEventArgs foundString = e as FoundStringEventArgs;
                if (foundString != null && foundString.LineToCheck.StartsWith(pair.Key))
                {
                    foundString.CallbackWasCalled = true;
                    pair.Value(this, e);
                }
            }
        }
    }

    public class FoundStringContainsCallbacks : FoundStringCallBacks
    {

        public void CheckForKeys(EventArgs e)
        {
            foreach (KeyValuePair<string, FoundStringEventHandler> pair in this.dictionaryOfCallBacks)
            {
                FoundStringEventArgs foundString = e as FoundStringEventArgs;
                if (foundString != null && foundString.LineToCheck.Contains(pair.Key))
                {
                    foundString.CallbackWasCalled = true;
                    pair.Value(this, e);
                }
            }
        }
    }
}