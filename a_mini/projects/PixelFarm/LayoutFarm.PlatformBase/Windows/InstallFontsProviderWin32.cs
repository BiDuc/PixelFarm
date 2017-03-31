﻿//MIT, 2014-2017, WinterDev   

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using PixelFarm.Drawing.Fonts;
namespace PixelFarm.Drawing
{

    /// <summary>
    /// provide installed font from Windows directory
    /// </summary>
    public class InstallFontsProviderWin32 : IInstalledFontProvider
    {
        public IEnumerable<string> GetInstalledFontIter()
        {
            //-------------------------------------------------
            //TODO: review here, this is not platform depend
            //-------------------------------------------------
            //check if MAC or linux font folder too
            //-------------------------------------------------
            string[] localMachineFonts = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", false).GetValueNames();
            // get parent of System folder to have Windows folder
            DirectoryInfo dirWindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));
            string strFontsFolder = Path.Combine(dirWindowsFolder.FullName, "Fonts");
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts");
            //---------------------------------------- 
            foreach (string winFontName in localMachineFonts)
            {
                string f = (string)regKey.GetValue(winFontName);
                if (f.EndsWith(".ttf") || f.EndsWith(".otf"))
                {
                    yield return Path.Combine(strFontsFolder, f);
                }
            }
        }

    }
}