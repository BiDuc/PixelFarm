﻿//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.CpuBlit.VertexProcessing
{
    using PixelFarm.Drawing;

    //-----------------------------------
    public struct VxsContext1 : IDisposable
    {
        internal readonly VertexStore vxs;
        internal VxsContext1(out VertexStore outputVxs)
        {
            VxsTemp.GetFreeVxs(out outputVxs);
            vxs = outputVxs;

        }
        public void Dispose()
        {
            VxsTemp.ReleaseVxs(vxs);
        }
    }
    public struct VxsContext2 : IDisposable
    {
        internal readonly VertexStore vxs1;
        internal readonly VertexStore vxs2;
        internal VxsContext2(out VertexStore outputVxs1, out VertexStore outputVxs2)
        {
            VxsTemp.GetFreeVxs(out vxs1);
            VxsTemp.GetFreeVxs(out vxs2);
            outputVxs1 = vxs1;
            outputVxs2 = vxs2;
        }
        public void Dispose()
        {
            //release
            VxsTemp.ReleaseVxs(vxs1);
            VxsTemp.ReleaseVxs(vxs2);
        }
    }
    public struct VxsContext3 : IDisposable
    {
        internal readonly VertexStore vxs1;
        internal readonly VertexStore vxs2;
        internal readonly VertexStore vxs3;
        internal VxsContext3(out VertexStore outputVxs1, out VertexStore outputVxs2, out VertexStore outputVxs3)
        {
            VxsTemp.GetFreeVxs(out vxs1);
            VxsTemp.GetFreeVxs(out vxs2);
            VxsTemp.GetFreeVxs(out vxs3);
            outputVxs1 = vxs1;
            outputVxs2 = vxs2;
            outputVxs3 = vxs3;

        }
        public void Dispose()
        {
            //release
            VxsTemp.ReleaseVxs(vxs1);
            VxsTemp.ReleaseVxs(vxs2);
            VxsTemp.ReleaseVxs(vxs3);
        }
    }


    //--------------------------------------------------

    public struct TempContext<T> : IDisposable
    {
        internal readonly T vxs;
        internal TempContext(out T outputvxs)
        {
            Temp<T>.GetFreeOne(out vxs);
            outputvxs = this.vxs;
        }
        public void Dispose()
        {
            Temp<T>.Release(vxs);
        }
    }

    public static class Temp<T>
    {
        [System.ThreadStatic]
        static Stack<T> s_pool;
        static Func<T> s_newHandler;
        static Action<T> s_releaseCleanUp;
        //-------

        public static TempContext<T> Borrow(out T freeItem)
        {
            return new TempContext<T>(out freeItem);
        }

        public static void SetNewHandler(Func<T> newHandler, Action<T> releaseCleanUp = null)
        {
            //set new instance here, must set this first***
            if (s_pool == null)
            {
                s_pool = new Stack<T>();
            }
            s_newHandler = newHandler;
            s_releaseCleanUp = releaseCleanUp;
        }
        internal static void GetFreeOne(out T freeItem)
        {
            if (s_pool.Count > 0)
            {
                freeItem = s_pool.Pop();
            }
            else
            {
                freeItem = s_newHandler();
            }
        }
        internal static void Release(T item)
        {
            s_releaseCleanUp?.Invoke(item);
            s_pool.Push(item);
            //... 
        }
        public static bool IsInit()
        {
            return s_pool != null;
        }
    }


}
namespace PixelFarm.Drawing
{

    public static class VxsTemp
    {

        public static VxsContext1 Borrow(out VertexStore vxs)
        {
            return new VxsContext1(out vxs);
        }
        public static VxsContext2 Borrow(out VertexStore vxs1, out VertexStore vxs2)
        {
            return new VxsContext2(out vxs1, out vxs2);
        }
        public static VxsContext3 Borrow(out VertexStore vxs1,
            out VertexStore vxs2, out VertexStore vxs3)
        {
            return new VxsContext3(out vxs1, out vxs2, out vxs3);
        }


        //for net20 -- check this
        //TODO: https://stackoverflow.com/questions/18333885/threadstatic-v-s-threadlocalt-is-generic-better-than-attribute

        [System.ThreadStatic]
        static Stack<VertexStore> s_vxsPool = new Stack<VertexStore>();

        internal static void GetFreeVxs(out VertexStore vxs1)
        {
            vxs1 = GetFreeVxs();
        }
        internal static void ReleaseVxs(VertexStore vxs1)
        {
            vxs1.Clear();
            s_vxsPool.Push(vxs1);
        }
        static VertexStore GetFreeVxs()
        {
            if (s_vxsPool == null)
            {
                s_vxsPool = new Stack<VertexStore>();
            }
            if (s_vxsPool.Count > 0)
            {
                return s_vxsPool.Pop();
            }
            else
            {
                return new VertexStore();
            }
        }
    }



    public static class VectorToolBox
    {

        public static TempContext<Stroke> Borrow(out Stroke stroke)
        {
            if (!Temp<Stroke>.IsInit())
            {
                Temp<Stroke>.SetNewHandler(() => new Stroke(1),
                    s => s.Width = 1);//reset?
            }
            return Temp<Stroke>.Borrow(out stroke);
        }
        public static TempContext<PixelFarm.CpuBlit.PathWriter> Borrow(out PixelFarm.CpuBlit.PathWriter pathWriter)
        {
            if (!Temp<PixelFarm.CpuBlit.PathWriter>.IsInit())
            {
                Temp<PixelFarm.CpuBlit.PathWriter>.SetNewHandler(
                    () => new PixelFarm.CpuBlit.PathWriter(),
                    w => w.Clear());
            }
            return Temp<PixelFarm.CpuBlit.PathWriter>.Borrow(out pathWriter);
        }
        public static TempContext<Ellipse> Borrow(out Ellipse pathWriter)
        {
            if (!Temp<Ellipse>.IsInit())
            {
                Temp<Ellipse>.SetNewHandler(() => new Ellipse());
            }
            return Temp<Ellipse>.Borrow(out pathWriter);
        }
        public static TempContext<SimpleRect> Borrow(out SimpleRect simpleRect)
        {
            if (!Temp<SimpleRect>.IsInit())
            {
                Temp<SimpleRect>.SetNewHandler(() => new SimpleRect());
            }
            return Temp<SimpleRect>.Borrow(out simpleRect);
        }
        public static TempContext<RoundedRect> Borrow(out RoundedRect roundRect)
        {
            if (!Temp<RoundedRect>.IsInit())
            {
                Temp<RoundedRect>.SetNewHandler(() => new RoundedRect());
            }
            return Temp<RoundedRect>.Borrow(out roundRect);
        }
        public static TempContext<VxsClipper> Borrow(out VxsClipper clipper)
        {
            if (!Temp<VxsClipper>.IsInit())
            {
                Temp<VxsClipper>.SetNewHandler(
                    () => new VxsClipper(),
                    c => c.Reset());
            }
            return Temp<VxsClipper>.Borrow(out clipper);
        }
         

    }
}
