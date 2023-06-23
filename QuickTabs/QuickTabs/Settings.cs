using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal abstract class Settings
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
            } else
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
            } catch
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
            } else
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
                    newValues[i] = prefsJson[field.Key].Value<JValue>().Value;
                } else
                {
                    if (migrateMode)
                    {
                        newValues[i] = Defaults[i];
                    } else
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
    internal class QTSettings : Settings
    {
        private QTSettings() : base() { }

        private static QTSettings current = new QTSettings();
        public static QTSettings Current { get => current; }

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
                }
                return prototype;
            }
        }
        private object[] defaults = new object[6] { true, true, false, false, true, true };
        protected override object[] Defaults => defaults;

        public bool ViewFretCounter
        {
            get
            {
                Initialize();
                return (bool)Values[0];
            } set
            {
                Initialize();
                Values[0] = value;
            }
        }
        public bool ViewNavDots
        {
            get
            {
                Initialize();
                return (bool)Values[1];
            } set
            {
                Initialize();
                Values[1] = value;
            }
        }
        public bool ViewCompactCtxMenu
        {
            get
            {
                Initialize();
                return (bool)Values[2];
            }
            set
            {
                Initialize();
                Values[2] = value;
            }
        }
        public bool ViewLargeFretboard
        {
            get
            {
                Initialize();
                return (bool)Values[3];
            }
            set
            {
                Initialize();
                Values[3] = value;
            }
        }
        public bool ViewDarkMode
        {
            get
            {
                Initialize();
                return (bool)Values[4];
            }
            set
            {
                Initialize();
                Values[4] = value;
            }
        }
        public bool ScaleAskAboutLoss
        {
            get
            {
                Initialize();
                return (bool)Values[5];
            }
            set
            {
                Initialize();
                Values[5] = value;
            }
        }
    }
}
