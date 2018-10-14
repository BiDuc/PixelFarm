﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using Typography.TextBreak;

namespace LayoutFarm.Composers
{
    public class MyManagedTextBreaker : ITextBreaker
    {
        CustomBreaker myTextBreaker;
        public MyManagedTextBreaker()
        {
            //TODO: review config folder here            
            myTextBreaker = CustomBreakerBuilder.NewCustomBreaker();
        }
        public void DoBreak(char[] inputBuffer, int startIndex, int len, List<int> breakAtList)
        {
            myTextBreaker.BreakWords(inputBuffer, startIndex, len);
            myTextBreaker.CopyBreakResults(breakAtList);

        }
        public void DoBreak(char[] inputBuffer, int startIndex, int len, List<WordBreakInfo> breakAtList)
        {

        }
    }
}