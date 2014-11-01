/*
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using MatterHackers.Agg.Transform;
using MatterHackers.Agg.Image;

namespace MatterHackers.Agg.UI
{
    public interface IGuiFactory
    {
        AbstractOsMappingWidget CreateSurface(SystemWindow childSystemWindow);
    }

    public static class OsMappingWidgetFactory
    {
        static IGuiFactory factoryToUse;

        public static void SetFactory(IGuiFactory factoryToUse)
        {
            if (OsMappingWidgetFactory.factoryToUse != null)
            {
                throw new NotSupportedException("You can only set the graphics target one time in an application.");
            }

            OsMappingWidgetFactory.factoryToUse = factoryToUse;
        }

        static AbstractOsMappingWidget primaryOsMappingWidget;
        public static AbstractOsMappingWidget PrimaryOsMappingWidget
        {
            get
            {
                return primaryOsMappingWidget;
            }
        }

        public static AbstractOsMappingWidget CreateOsMappingWidget(SystemWindow childSystemWindow)
        {
            if (factoryToUse == null)
            {
                throw new NotSupportedException("You must call 'SetGuiBackend' with a GuiFactory before you can create any surfaces");
            }

            AbstractOsMappingWidget osMappingWidget = factoryToUse.CreateSurface(childSystemWindow);
            if (primaryOsMappingWidget == null)
            {
                primaryOsMappingWidget = osMappingWidget;
            }

            return osMappingWidget;
        }
    }
}
