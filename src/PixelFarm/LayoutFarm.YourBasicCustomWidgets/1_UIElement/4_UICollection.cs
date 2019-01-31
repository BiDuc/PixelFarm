﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
namespace LayoutFarm.UI
{
    public class UICollection
    {
        List<UIElement> _uiList = new List<UIElement>();
        UIElement _owner;
        public UICollection(UIElement owner)
        {
            _owner = owner;
        }
        //
        public UIElement GetElement(int index) => _uiList[index];
        public int Count => _uiList.Count;
        //
        public int FindIndex(UIElement ui)
        {
            return _uiList.IndexOf(ui);
        }
        public void InsertUI(int index, UIElement newUI)
        {
            _uiList.Insert(index, newUI);
        }
        public void AddUI(UIElement ui)
        {
#if DEBUG
            if (_owner == ui)
                throw new Exception("cyclic!");
#endif
            _uiList.Add(ui);
            ui.ParentUI = _owner;
        }
        public bool RemoveUI(UIElement ui)
        {
            //remove specific ui 
            if (_uiList.Remove(ui))
            {
                ui.ParentUI = null;//clear parent
                return true;
            }
            else
            {
                return false;
            }
        }
        public void RemoveAt(int index)
        {
            UIElement ui = _uiList[index];
            _uiList.RemoveAt(index);
            ui.ParentUI = null;
        }
        public void Clear()
        {
            //clear all parent relation
            for (int i = _uiList.Count - 1; i >= 0; --i)
            {
                _uiList[i].ParentUI = null;
            }
            _uiList.Clear();
        }

    }
}