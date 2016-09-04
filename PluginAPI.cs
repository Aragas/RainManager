/*
  Copyright (C) 2011 Birunthan Mohanathas

  This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using System;
using System.Runtime.InteropServices;

namespace RainManager
{
    /// <summary>
    /// Wrapper around the Rainmeter Plugin C API.
    /// </summary>
    public static class PluginAPI
    {
        private static IntPtr StringBuffer = IntPtr.Zero;

        [DllExport]
        public static void Initialize(ref IntPtr measurePtr, IntPtr apiPtr)
        {
            RainmeterAPI api = new RainmeterAPI(apiPtr);
            RainmeterSkinHandler skinHandler = RainmeterSkinHandler.GetSkinHandler(api);
            skinHandler.M_Initialize(ref measurePtr, api);
            RainmeterSkinHandler.AddMeasurePtr(measurePtr, skinHandler);
        }

        [DllExport]
        public static void Reload(IntPtr measurePtr, IntPtr apiPtr, ref double maxValue)
        {
            RainmeterSkinHandler skinHandler = RainmeterSkinHandler.GetSkinHandlerByMeasurePtr(measurePtr);
            skinHandler.M_Reload(measurePtr, new RainmeterAPI(apiPtr), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr measurePtr)
        {
            RainmeterSkinHandler skinHandler = RainmeterSkinHandler.GetSkinHandlerByMeasurePtr(measurePtr);
            return skinHandler.M_GetNumeric(measurePtr);
        }

        [DllExport]
        public static IntPtr GetString(IntPtr measurePtr)
        {
            RainmeterSkinHandler skinHandler = RainmeterSkinHandler.GetSkinHandlerByMeasurePtr(measurePtr);
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = skinHandler.M_GetString(measurePtr);
            if (stringValue != null)
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);

            return StringBuffer;
        }

        [DllExport]
        public static void ExecuteBang(IntPtr measurePtr, IntPtr argsPtr)
        {
            RainmeterSkinHandler skinHandler = RainmeterSkinHandler.GetSkinHandlerByMeasurePtr(measurePtr);
            skinHandler.M_ExecuteBang(measurePtr, Marshal.PtrToStringUni(argsPtr));
        }

        [DllExport]
        public static void Finalize(IntPtr measurePtr)
        {
            RainmeterSkinHandler skinHandler = RainmeterSkinHandler.GetSkinHandlerByMeasurePtr(measurePtr);
            skinHandler.M_Finalize(measurePtr);

            if (measurePtr != IntPtr.Zero)
                GCHandle.FromIntPtr(measurePtr).Free();

            RainmeterSkinHandler.RemoveMeasurePtr(measurePtr);

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }
    }
}
