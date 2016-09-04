﻿/*
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
    /// Wrapper around the Rainmeter C API.
    /// </summary>
    public class RainmeterAPI
    {
        private IntPtr m_Rm;

        public RainmeterAPI(IntPtr rm) { m_Rm = rm; }

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private extern static IntPtr RmReadString(IntPtr rm, string option, string defValue, bool replaceMeasures);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private extern static double RmReadFormula(IntPtr rm, string option, double defValue);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private extern static IntPtr RmReplaceVariables(IntPtr rm, string str);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private extern static IntPtr RmPathToAbsolute(IntPtr rm, string relativePath);

        [DllImport("Rainmeter.dll", EntryPoint = "RmExecute", CharSet = CharSet.Unicode)]
        public extern static void Execute(IntPtr skin, string command);

        [DllImport("Rainmeter.dll")]
        private extern static IntPtr RmGet(IntPtr rm, RmGetType type);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private extern static int LSLog(LogType type, string unused, string message);

        private enum RmGetType
        {
            MeasureName = 0,
            Skin = 1,
            SettingsFile = 2,
            SkinName = 3,
            SkinWindowHandle = 4
        }

        public enum LogType
        {
            Error = 1,
            Warning = 2,
            Notice = 3,
            Debug = 4
        }

        public string ReadString(string option, string defValue, bool replaceMeasures = true) => Marshal.PtrToStringUni(RmReadString(m_Rm, option, defValue, replaceMeasures));

        public string ReadPath(string option, string defValue) => Marshal.PtrToStringUni(RmPathToAbsolute(m_Rm, ReadString(option, defValue)));

        public double ReadDouble(string option, double defValue) => RmReadFormula(m_Rm, option, defValue);

        public int ReadInt(string option, int defValue) => (int) RmReadFormula(m_Rm, option, defValue);

        public string ReplaceVariables(string str) => Marshal.PtrToStringUni(RmReplaceVariables(m_Rm, str));

        public string GetMeasureName() => Marshal.PtrToStringUni(RmGet(m_Rm, RmGetType.MeasureName));

        public IntPtr GetSkin() => RmGet(m_Rm, RmGetType.Skin);

        public string GetSettingsFile() => Marshal.PtrToStringUni(RmGet(m_Rm, RmGetType.SettingsFile));

        public string GetSkinName() => Marshal.PtrToStringUni(RmGet(m_Rm, RmGetType.SkinName));

        public IntPtr GetSkinWindow() => RmGet(m_Rm, RmGetType.SkinWindowHandle);

        public static void Log(LogType type, string message) => LSLog(type, null, message);
    }
}
