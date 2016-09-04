using System;
using System.Collections.Generic;
using System.IO;

namespace RainManager
{
    public abstract class PluginSkin : IDisposable
    {
        #region MeasurePtr->PluginMeasure Reference.
        private Dictionary<IntPtr, PluginMeasure> PluginMeasureByMeasurePtr = new Dictionary<IntPtr, PluginMeasure>();
        internal bool IsEmpty => PluginMeasureByMeasurePtr.Count == 0;
        internal void AddPluginMeasure(IntPtr measurePtr, PluginMeasure pluginMeasure) => PluginMeasureByMeasurePtr.Add(measurePtr, pluginMeasure);
        internal void RemovePluginMeasure(IntPtr measurePtr) => PluginMeasureByMeasurePtr.Remove(measurePtr);
        internal PluginMeasure GetPluginMeasure(IntPtr measurePtr) => PluginMeasureByMeasurePtr[measurePtr];
        #endregion MeasurePtr->PluginMeasure Reference.

        public RainmeterSkinHandler SkinHandler { get; }
        public List<PluginMeasure> PluginMeasures => new List<PluginMeasure>(PluginMeasureByMeasurePtr.Values);

        public IntPtr Ptr => SkinHandler.SkinPtr;
        public string Name => SkinHandler.Name;
        public IntPtr WindowPtr => SkinHandler.WindowPtr;
        public string Path { get; }

        public PluginSkin(RainmeterSkinHandler skinHandler, RainmeterAPI api)
        {
            SkinHandler = skinHandler;

            // -- We need to remove the last directory from path.
            var path = api.ReadPath(RainmeterSkinHandler.PluginMeasureName, string.Empty);
            var dir = Directory.GetParent(path.EndsWith("\\") ? path : string.Concat(path, "\\"));
            Path = dir.Parent.FullName;
        }

        public abstract void Dispose();

        public override string ToString() => $"{Name}[{Ptr}]";
    }
}
