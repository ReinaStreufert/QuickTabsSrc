using Newtonsoft.Json.Linq;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public abstract class Persistence
    {
        protected object[] Values = null;
        protected bool IsInitialized = false;

        protected abstract Dictionary<string, JTokenType> Prototype { get; }
        protected abstract object[] Defaults { get; }

        public static string QuickTabsDataDir
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string qtData = Path.Combine(appData, "QuickTabs");
                if (!Directory.Exists(qtData))
                {
                    Directory.CreateDirectory(qtData);
                }
                return qtData;
            }
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            string qtData = QuickTabsDataDir;
            string prefsPath = Path.Combine(qtData, "prefs.json");
            if (File.Exists(prefsPath))
            {
                loadPrefs(prefsPath);
            }
            else
            {
                Save(); // save default
            }
            IsInitialized = true;
        }
        public void Save()
        {
            if (Values == null)
            {
                Values = new object[Defaults.Length];
                Defaults.CopyTo(Values, 0);
            }
            JObject prefsJson = new JObject();
            prefsJson.Add("qtver", Updater.SelfReleaseVersion);

            Dictionary<string, JTokenType> prototype = Prototype;
            int i = 0;
            foreach (KeyValuePair<string, JTokenType> field in prototype)
            {
                JToken value = JToken.FromObject(Values[i]);
                if (value.Type != field.Value)
                {
                    throw new InvalidCastException("Values did not match prototype");
                }
                prefsJson.Add(field.Key, value);
                i++;
            }

            string qtData = QuickTabsDataDir;
            string prefsPath = Path.Combine(qtData, "prefs.json");

            if (File.Exists(prefsPath))
            {
                File.Delete(prefsPath);
            }
            File.WriteAllText(prefsPath, prefsJson.ToString());
        }
        protected void Migrate(JObject prefsJson) { }
        private void loadPrefs(string prefsPath)
        {
            string prefs = File.ReadAllText(prefsPath);

            JObject prefsJson;
            try
            {
                prefsJson = JObject.Parse(prefs);
            }
            catch
            {
                Save(); // save default
                return;
            }
            // validate version
            bool migrateMode = false;
            if (prefsJson.ContainsKey("qtver") && prefsJson["qtver"].Type == JTokenType.Integer)
            {
                int prefsVersion = (int)prefsJson["qtver"];
                if (prefsVersion < Updater.SelfReleaseVersion)
                {
                    migrateMode = true;
                    Migrate(prefsJson);
                }
            }
            else
            {
                Save(); // save default
                return;
            }
            // validate that prefs.json matches prototype and copy values
            Dictionary<string, JTokenType> prototype = Prototype;
            object[] newValues = new object[prototype.Count];
            int i = 0;
            foreach (KeyValuePair<string, JTokenType> field in prototype)
            {
                if (prefsJson.ContainsKey(field.Key) && prefsJson[field.Key].Type == field.Value)
                {
                    if (field.Value == JTokenType.Array)
                    {
                        JArray jArray = (JArray)(prefsJson[field.Key]);
                        object[] arr = new object[jArray.Count];
                        int ii = 0;
                        foreach (JToken token in jArray)
                        {
                            arr[ii] = token.Value<JValue>().Value;
                            ii++;
                        }
                        newValues[i] = arr;
                    } else
                    {
                        newValues[i] = prefsJson[field.Key].Value<JValue>().Value;
                    }
                }
                else
                {
                    if (migrateMode)
                    {
                        newValues[i] = Defaults[i];
                    }
                    else
                    {
                        Save(); // save default
                        return;
                    }
                }
                i++;
            }
            Values = newValues;
            if (migrateMode)
            {
                Save();
            }
        }
    }
    public class QTPersistence : Persistence
    {
        private QTPersistence() : base() { }

        private static QTPersistence current = new QTPersistence();
        public static QTPersistence Current { get => current; }

        public static class Keys
        {
            public const int ViewFretCounter = 0;
            public const int ViewNavDots = 1;
            public const int ViewCompactCtxMenu = 2;
            public const int ViewLargeFretboard = 3;
            public const int ViewDarkMode = 4;
            public const int ScaleAskAboutLoss = 5;
            public const int ViewMidLines = 6;
            public const int ConfigPinnedPrefs = 7;
            public const int MidlineInterval = 8;
            public const int ScaleMergeSteps = 9;
            public const int AsioOutputDevice = 10;
            public const int EnableAutoUpdate = 11;
            public const int EnablePreviewPlay = 12;
            public const int AutosaveOnCrash = 13;
            public const int AutorestartOnCrash = 14;
            public const int LargestKey = AutorestartOnCrash;
        }

        private Dictionary<string, JTokenType> prototype = null;
        protected override Dictionary<string, JTokenType> Prototype
        {
            get
            {
                if (prototype == null)
                {
                    prototype = new Dictionary<string, JTokenType>();
                    prototype["ViewFretCounter"] = JTokenType.Boolean;
                    prototype["ViewNavDots"] = JTokenType.Boolean;
                    prototype["ViewCompactCtxMenu"] = JTokenType.Boolean;
                    prototype["ViewLargeFretboard"] = JTokenType.Boolean;
                    prototype["ViewDarkMode"] = JTokenType.Boolean;
                    prototype["ScaleAskAboutLoss"] = JTokenType.Boolean;
                    prototype["ViewMidLines"] = JTokenType.Boolean;
                    prototype["ConfigPinnedPrefs"] = JTokenType.Array;
                    prototype["MidlineInterval"] = JTokenType.Integer;
                    prototype["ScaleMergeSteps"] = JTokenType.Boolean;
                    prototype["AsioOutputDevice"] = JTokenType.Integer;
                    prototype["EnableAutoUpdate"] = JTokenType.Boolean;
                    prototype["EnablePreviewPlay"] = JTokenType.Boolean;
                    prototype["AutosaveOnCrash"] = JTokenType.Boolean;
                    prototype["AutorestartOnCrash"] = JTokenType.Boolean;
                }
                return prototype;
            }
        }
        private object[] defaults = null;
        protected override object[] Defaults
        {
            get
            {
                if (defaults == null)
                {
                    defaults = new object[Keys.LargestKey + 1];
                    defaults[Keys.ViewFretCounter] = true;
                    defaults[Keys.ViewNavDots] = true;
                    defaults[Keys.ViewCompactCtxMenu] = false;
                    defaults[Keys.ViewLargeFretboard] = false;
                    defaults[Keys.ViewDarkMode] = true;
                    defaults[Keys.ScaleAskAboutLoss] = true;
                    defaults[Keys.ViewMidLines] = false;
                    defaults[Keys.ConfigPinnedPrefs] = new object[] { "Show mid lines" };
                    defaults[Keys.MidlineInterval] = (long)((new MusicalTimespan(1, 4)).SerializeToInt32());
                    defaults[Keys.ScaleMergeSteps] = false;
                    defaults[Keys.AsioOutputDevice] = (long)0; // 0 always equals default audio output device
                    defaults[Keys.EnableAutoUpdate] = true;
                    defaults[Keys.EnablePreviewPlay] = true;
                    defaults[Keys.AutosaveOnCrash] = true;
                    defaults[Keys.AutorestartOnCrash] = true;
                }
                return defaults;
            }
        }

        public object this[int key]
        {
            get
            {
                Initialize();
                return Values[key];
            }
            set
            {
                Initialize();
                Values[key] = value;
            }
        }

        public bool ViewFretCounter
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ViewFretCounter];
            }
            set
            {
                Initialize();
                Values[Keys.ViewFretCounter] = value;
            }
        }
        public bool ViewNavDots
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ViewNavDots];
            }
            set
            {
                Initialize();
                Values[Keys.ViewNavDots] = value;
            }
        }
        public bool ViewCompactCtxMenu
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ViewCompactCtxMenu];
            }
            set
            {
                Initialize();
                Values[Keys.ViewCompactCtxMenu] = value;
            }
        }
        public bool ViewLargeFretboard
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ViewLargeFretboard];
            }
            set
            {
                Initialize();
                Values[Keys.ViewLargeFretboard] = value;
            }
        }
        public bool ViewDarkMode
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ViewDarkMode];
            }
            set
            {
                Initialize();
                Values[Keys.ViewDarkMode] = value;
            }
        }
        public bool ScaleAskAboutLoss
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ScaleAskAboutLoss];
            }
            set
            {
                Initialize();
                Values[Keys.ScaleAskAboutLoss] = value;
            }
        }
        public bool ViewMidLines
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ViewMidLines];
            }
            set
            {
                Initialize();
                Values[Keys.ViewMidLines] = value;
            }
        }
        public string[] ConfigPinnedPrefs
        {
            get
            {
                Initialize();
                object[] val = (object[])Values[Keys.ConfigPinnedPrefs];
                string[] converted = new string[val.Length];
                for (int i = 0; i < val.Length; i++)
                {
                    converted[i] = (string)val[i];
                }
                return converted;
            }
            set
            {
                Initialize();
                object[] converted = new object[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    converted[i] = value[i];
                }
                Values[Keys.ConfigPinnedPrefs] = converted;
            }
        }
        public MusicalTimespan MidlineInterval
        {
            get
            {
                Initialize();
                return MusicalTimespan.DeserializeInt32(Convert.ToInt32((long/*why*/)Values[Keys.MidlineInterval]));
            }
            set
            {
                Initialize();
                Values[Keys.MidlineInterval] = (long/*why*/)value.SerializeToInt32();
            }
        }
        public bool ScaleMergeSteps
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.ScaleMergeSteps];
            }
            set
            {
                Initialize();
                Values[Keys.ScaleMergeSteps] = value;
            }
        }
        public long AsioOutputDevice
        {
            get
            {
                Initialize();
                return (long)Values[Keys.AsioOutputDevice];
            }
            set
            {
                Initialize();
                Values[Keys.AsioOutputDevice] = value;
            }
        }
        public bool EnableAutoUpdate
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.EnableAutoUpdate];
            }
            set
            {
                Initialize();
                Values[Keys.EnableAutoUpdate] = value;
            }
        }
        public bool EnablePreviewPlay
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.EnablePreviewPlay];
            }
            set
            {
                Initialize();
                Values[Keys.EnablePreviewPlay] = value;
            }
        }
        public bool AutosaveOnCrash
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.AutosaveOnCrash];
            }
            set
            {
                Initialize();
                Values[Keys.AutosaveOnCrash] = value;
            }
        }
        public bool AutorestartOnCrash
        {
            get
            {
                Initialize();
                return (bool)Values[Keys.AutorestartOnCrash];
            }
            set
            {
                Initialize();
                Values[Keys.AutorestartOnCrash] = value;
            }
        }
    }
}
