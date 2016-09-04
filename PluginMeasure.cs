using System;
using System.Reflection;

namespace RainManager
{
    /// <summary>
    /// A Measure that will return any value requested by Rainmeter. Can receive command via ExecuteBang(COMMAND).
    /// NOTE: Priority is set to GetString(). If return value is numric, return null in GetString(). If you won't do it, GetNumeric() won't be called.
    /// </summary>
    public abstract class PluginMeasure : IDisposable
    {
        public string Name { get; }

        public PluginMeasure(RainmeterAPI api)
        {
            Name = api.GetMeasureName();
        }

        public abstract void Reload(RainmeterAPI api, ref double maxValue);
        public abstract double GetNumeric();
        public abstract string GetString();
        public abstract void ExecuteBang(string command);
        public abstract void Dispose();
    }
    public abstract class PluginMeasure<TSkin> : PluginMeasure where TSkin : PluginSkin
    {
        public TSkin Skin { get; }

        public string Path => Skin.Path;

        public PluginMeasure(TSkin skin, RainmeterAPI api) : base(api)
        {
            Skin = skin;
        }
    }
    public abstract class PluginMeasure<TSkin, TEnum> : PluginMeasure<TSkin> where TSkin : PluginSkin where TEnum : struct, IConvertible
    {
        public TEnum TypeEnum { get; }

        public PluginMeasure(string measureType, TSkin skin, RainmeterAPI api) : base(skin, api)
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an enumerated type");

            TEnum typeEnum = default(TEnum);
            if (!Enum.TryParse(measureType, true, out typeEnum))
                RainmeterAPI.Log(RainmeterAPI.LogType.Error, $"{System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location)} {RainmeterSkinHandler.PluginMeasureType}={measureType} not valid.");
            TypeEnum = typeEnum;
        }

        public override string ToString() => $"{GetType().Name.Replace("Measure", "")}[{TypeEnum.ToString()}]";
    }
}
