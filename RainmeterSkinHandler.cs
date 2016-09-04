using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RainManager
{
    /// <summary>
    /// Each instance of this class represents one Rainmeter Skin. This feature allow us to use one plugin for multiple skins, without a static Measure handler. See examples.
    /// </summary>
    public class RainmeterSkinHandler
    {
        /// <summary>
        /// Specifies which Measure should be used.
        /// </summary>
        internal const string PluginAssemblyName = "PluginAssemblyName";
        /// <summary>
        /// Specifies which Measure should be used.
        /// </summary>
        internal const string PluginMeasureName = "PluginMeasureName";
        /// <summary>
        /// Specifies which type in a Measure should be used.
        /// </summary>
        internal const string PluginMeasureType = "PluginMeasureType";

        /// <summary>
        /// Used when received an IntPtr.Zero Measure pointer.
        /// </summary>
        private static RainmeterSkinHandler Empty { get; } = new RainmeterSkinHandler(IntPtr.Zero, "EMPTY_SKIN", IntPtr.Zero);

        #region Assembly Dependency Resolver. Store any required by plugin .dll in %MY_DOCUMENTS%\Rainmeter\Skins\%SKIN%.
        static RainmeterSkinHandler() { AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve; }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // -- Basically, will try find any needed .dll in every Skin that is using the plugin.
            foreach (var skinHandlers in SkinHandlerBySkinPtr.Values)
                foreach (var pluginSkin in skinHandlers.PluginSkinByTypes.Values)
                {
                    var path = Path.Combine(pluginSkin.Path, $"{new AssemblyName(args.Name).Name}.dll");
                    if (File.Exists(path))
                        return Assembly.LoadFrom(path);
                }

            // -- Resolve RainMamager.dll dependency (lol).
            var caller = Assembly.GetAssembly(typeof(RainmeterSkinHandler));
            if (new AssemblyName(args.Name).Name == caller.GetName().Name)
                return caller;

            return null;
        }
        #endregion Assembly Dependency Resolver. Store any required .dll in %MY_DOCUMENTS%\Rainmeter\Skins\%SKIN%.

        #region Fast SkinPtr->SkinHandler Reference.
        private static Dictionary<IntPtr, RainmeterSkinHandler> SkinHandlerBySkinPtr = new Dictionary<IntPtr, RainmeterSkinHandler>();
        public static List<RainmeterSkinHandler> SkinHandlers => new List<RainmeterSkinHandler>(SkinHandlerBySkinPtr.Values);
        internal static RainmeterSkinHandler GetSkinHandler(RainmeterAPI api)
        {
            IntPtr skinPtr = api.GetSkin();

            RainmeterSkinHandler skinHandler;
            if (!SkinHandlerBySkinPtr.TryGetValue(skinPtr, out skinHandler))
                SkinHandlerBySkinPtr.Add(skinPtr, (skinHandler = new RainmeterSkinHandler(skinPtr, api.GetSkinName(), api.GetSkinWindow())));
            return skinHandler;
        }
        #endregion Fast SkinPtr->SkinHandler Reference.

        #region Fast MeasurePtr->SkinHandler Reference.
        private static Dictionary<IntPtr, RainmeterSkinHandler> SkinHandlerByMeasurePtr = new Dictionary<IntPtr, RainmeterSkinHandler>();
        internal static RainmeterSkinHandler GetSkinHandlerByMeasurePtr(IntPtr measurePtr)
        {
            if (measurePtr == IntPtr.Zero)
                return Empty;

            return SkinHandlerByMeasurePtr[measurePtr];
        }
        internal static void AddMeasurePtr(IntPtr measurePtr, RainmeterSkinHandler skinHandler)
        {
            if (measurePtr == IntPtr.Zero)
                return;

            SkinHandlerByMeasurePtr.Add(measurePtr, skinHandler);
        }
        internal static void RemoveMeasurePtr(IntPtr measurePtr)
        {
            if (measurePtr == IntPtr.Zero)
                return;

            SkinHandlerByMeasurePtr.Remove(measurePtr);
        }
        #endregion Fast MeasurePtr->SkinHandler Reference.

        #region Creating Instances of Implemented Classes by Known Scheme.
        private static Assembly GetAssembly(string assemblyPath)
        {
            if (File.Exists(assemblyPath))
                return Assembly.LoadFrom(assemblyPath);

            return null;
        }

        private static Type GetPluginSkinType(Assembly assembly, string measureType) =>
            assembly.GetTypes().SingleOrDefault(type => type.Name.ToLowerInvariant() == $"{measureType}Skin".ToLowerInvariant());

        // -- Add Base-Derived class support.
        private static PluginSkin CreatePluginSkin(Assembly assembly, string measureType, RainmeterSkinHandler skinHandler, RainmeterAPI api) =>
            (PluginSkin) Activator.CreateInstance(GetPluginSkinType(assembly, measureType), new object[] { skinHandler, api });

        private static Type GetPluginMeasureType(Assembly assembly, string measureType) =>
            assembly.GetTypes().SingleOrDefault(type => type.Name.ToLowerInvariant() == $"{measureType}Measure".ToLowerInvariant());

        // -- Add Base-Derived class support.
        private static PluginMeasure CreatePluginMeasure(Assembly assembly, string measureType, string pluginType, PluginSkin skin, RainmeterAPI api) =>
            (PluginMeasure) Activator.CreateInstance(GetPluginMeasureType(assembly, measureType), new object[] { pluginType, skin, api });
        #endregion Creating Instances of Implemented Classes by Known Scheme.


        #region Fast MeasurePtr->PluginSkin Reference.
        private Dictionary<IntPtr, PluginSkin> PluginSkinByMeasurePtr = new Dictionary<IntPtr, PluginSkin>();
        private PluginMeasure GetPluginMeasureType(IntPtr ptr) => PluginSkinByMeasurePtr[ptr].GetPluginMeasure(ptr);
        private void AddPluginMeasure(IntPtr ptr, PluginSkin skin, PluginMeasure pluginMeasure)
        {
            PluginSkinByMeasurePtr.Add(ptr, skin);
            PluginSkinByMeasurePtr[ptr].AddPluginMeasure(ptr, pluginMeasure);
        }
        private void RemovePluginMeasure(IntPtr ptr)
        {
            PluginSkinByMeasurePtr[ptr].RemovePluginMeasure(ptr);
            PluginSkinByMeasurePtr.Remove(ptr);
        }
        #endregion Fast MeasurePtr->PluginSkin Reference.

        public IntPtr SkinPtr { get; }
        public string Name { get; }
        public IntPtr WindowPtr { get; }

        internal Dictionary<Type, PluginSkin> PluginSkinByTypes = new Dictionary<Type, PluginSkin>();
        public List<PluginSkin> PluginSkins => new List<PluginSkin>(PluginSkinByTypes.Values);

        internal RainmeterSkinHandler(IntPtr ptr, string name, IntPtr windowPtr)
        {
            SkinPtr = ptr;
            Name = name;
            WindowPtr = windowPtr;
        }

        internal void   M_Initialize(ref IntPtr measurePtr, RainmeterAPI api)
        {
            var measureName = api.ReadString(PluginMeasureName, string.Empty);
            var measureType = api.ReadString(PluginMeasureType, string.Empty);

            var assemblyName = api.ReadString(PluginAssemblyName, string.Empty);
            var assemblyPath = api.ReadPath(PluginAssemblyName, string.Empty);
            var assembly = GetAssembly(assemblyPath);

            var measureTypeType = GetPluginMeasureType(assembly, measureName);
            if (measureTypeType == null) // -- Error checking.
            {
                if (string.IsNullOrEmpty(measureName))
                    RainmeterAPI.Log(RainmeterAPI.LogType.Error, $"{PluginMeasureName}= Not found.");

                if (string.IsNullOrEmpty(measureType))
                    RainmeterAPI.Log(RainmeterAPI.LogType.Error, $"{PluginMeasureType}= Not found.");

                if (!string.IsNullOrEmpty(measureName) && !string.IsNullOrEmpty(measureType))
                    RainmeterAPI.Log(RainmeterAPI.LogType.Error, $"Missing .dll's");

                // Not sure how bad this is on rainmeter side, but it seems fine. That's the only safe way to handle errors.
                // Pointer will be reset anyway by refreshin theg skin with fixed errors.
                measurePtr = IntPtr.Zero;
                return;
            }

            // -- Check if a PluginSkin exist for this type of MeasureType. Create and add if not.
            if (!PluginSkinByTypes.ContainsKey(measureTypeType))
            {
                var skin = CreatePluginSkin(assembly, measureName, this, api);
                PluginSkinByTypes.Add(measureTypeType, skin);
            }

            var pluginMeasure = CreatePluginMeasure(assembly, measureName, measureType, PluginSkinByTypes[measureTypeType], api);
            measurePtr = GCHandle.ToIntPtr(GCHandle.Alloc(pluginMeasure));
            AddPluginMeasure(measurePtr, PluginSkinByTypes[measureTypeType], pluginMeasure);
        }
        internal void   M_Reload(IntPtr measurePtr, RainmeterAPI api, ref double maxValue)
        {
            if (measurePtr == IntPtr.Zero)
                return;

            GetPluginMeasureType(measurePtr).Reload(api, ref maxValue);
        }
        internal double M_GetNumeric(IntPtr measurePtr)
        {
            if (measurePtr == IntPtr.Zero)
                return 0.0;

            return GetPluginMeasureType(measurePtr).GetNumeric();
        }
        internal string M_GetString(IntPtr measurePtr)
        {
            if (measurePtr == IntPtr.Zero)
                return string.Empty;

            return GetPluginMeasureType(measurePtr).GetString();
        }
        internal void   M_ExecuteBang(IntPtr measurePtr, string args)
        {
            if (measurePtr == IntPtr.Zero)
                return;

            GetPluginMeasureType(measurePtr).ExecuteBang(args);
        }
        internal void   M_Finalize(IntPtr measurePtr)
        {
            if (measurePtr == IntPtr.Zero)
                return;

            GetPluginMeasureType(measurePtr).Dispose();


            RemovePluginMeasure(measurePtr);

            List<Type> removeList = new List<Type>();
            foreach (var skin in PluginSkinByTypes)
            {
                if (skin.Value.IsEmpty)
                {
                    skin.Value.Dispose();
                    removeList.Add(skin.Key);
                }
            }
            foreach (var type in removeList)
                PluginSkinByTypes.Remove(type);

            if (PluginSkinByTypes.Count == 0)
                Dispose();
        }

        internal void Dispose()
        {
            SkinHandlerBySkinPtr.Remove(SkinPtr);
        }


        public override string ToString() => $"{Name}[{SkinPtr}]";
    }
}
