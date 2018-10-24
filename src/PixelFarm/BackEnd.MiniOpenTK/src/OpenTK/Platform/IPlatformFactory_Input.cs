﻿//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using OpenTK.Graphics;

namespace OpenTK.Platform
{
    internal interface IPlatformFactory2 : IPlatformFactory, IDisposable
    {
        //INativeWindow2 CreateNativeWindow(int x, int y, int width, int height, string title, GraphicsMode mode, GameWindowFlags options, DisplayDevice device);

        //IDisplayDeviceDriver CreateDisplayDeviceDriver();

        //IGraphicsContext CreateGLContext(GraphicsMode mode, IWindowInfo window, IGraphicsContext shareContext, bool directRendering, int major, int minor, GraphicsContextFlags flags);

        //IGraphicsContext CreateGLContext(ContextHandle handle, IWindowInfo window, IGraphicsContext shareContext, bool directRendering, int major, int minor, GraphicsContextFlags flags);

        //GraphicsContext.GetCurrentContextDelegate CreateGetCurrentGraphicsContext();

        OpenTK.Input.IKeyboardDriver2 CreateKeyboardDriver();

        OpenTK.Input.IMouseDriver2 CreateMouseDriver();

        OpenTK.Input.IGamePadDriver CreateGamePadDriver();

        Input.IJoystickDriver2 CreateJoystickDriver();

        //void RegisterResource(IDisposable resource);
    }
}