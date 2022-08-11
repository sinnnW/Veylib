/*
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Veylib.ICLI
{
    public class SettingsMenu
    {
        public Settings CurrentSettings;
        private int currentSettingIndex = 0;
        private int currentValueIndex = 0;
        private Core core = Core.GetInstance();

        public SettingsMenu()
        {
            CurrentSettings = new Settings();
        }

        public SettingsMenu(Settings settings) 
        {
            CurrentSettings = settings;
        }

        public struct SettingKeyValue {
            public string Key;
            public List<string> AllowedValues;
            public int DefaultValueIndex;
            public bool ReadOnly;
        }

        public class Settings
        {
            public List<SettingKeyValue> DisplayedSettings = new List<SettingKeyValue>();
            public Style Style;
        }

        public class Style
        {
            public Color HeaderColor = Color.White;
            public Color ContentColor = Color.WhiteSmoke;
            public Color PipeColor = Color.White;

            public Color NeutralColor = Color.WhiteSmoke;
            public Color SelectionHighlightColor = Color.White;
            public Color SelectedColor = Color.Lime;

            public string SelectionFormatTags = Core.Formatting.Underline;
            public string SelectedFormatTags = Core.Formatting.Underline;

            public string PreOptionText = "> ";
            public string PreOptionFormatTags = "";
            public string PreOptionHighlightFormatTags = "";
            public string PreOptionSelectedFormatTags = "";

            public Color PreOptionColor = Color.WhiteSmoke;
            public Color PreOptionHighlightColor = Color.White;
            public Color PreOptionSelectedColor = Color.WhiteSmoke;

            public bool RainbowDividers = false;
        }

        public void AddSetting(SettingKeyValue setting)
        {
            if (setting.AllowedValues?.Count == 0)
                throw new Exception("Must supply allowed values");

            CurrentSettings.DisplayedSettings.Add(setting);
        }

        public void AddSetting(string key, List<string> values, bool readOnly = false, int defaultValueIndex = 0)
        {
            if (values.Count == 0)
                throw new Exception("Must supply allowed values");

            var settings = new SettingKeyValue();
            settings.Key = key;
            settings.AllowedValues = values;
            settings.ReadOnly = readOnly;
            settings.DefaultValueIndex = defaultValueIndex;

            CurrentSettings.DisplayedSettings.Add(settings);
        }

        public override string ToString()
        {
            var cols = new List<AsciiTable.Column> { new AsciiTable.Column(null, "Setting"), new AsciiTable.Column(null, "Value") };

            var sb = new StringBuilder();

            sb.AppendLine(AsciiTable.TableBuilder.TopLine(cols));

            sb.AppendLine();

            string divider = AsciiTable.TableBuilder.Divider(cols);

            sb.AppendLine(AsciiTable.TableBuilder.BottomLine(cols));

            return sb.ToString();
        }

        public void Render()
        {

            

           

        }

        public void Activate()
        {
            core.DelayUntilReady();

            while (true)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.DownArrow:
                        currentSettingIndex++;
                        if (currentSettingIndex > CurrentSettings.DisplayedSettings.Count - 1)
                            currentSettingIndex--;
                        else
                            currentValueIndex = CurrentSettings.DisplayedSettings[currentSettingIndex].DefaultValueIndex;

                        break;
                    case ConsoleKey.UpArrow:
                        currentSettingIndex--;
                        if (currentSettingIndex < 0)
                            currentSettingIndex++;
                        else
                            currentValueIndex = CurrentSettings.DisplayedSettings[currentSettingIndex].DefaultValueIndex;

                        break;
                    case ConsoleKey.RightArrow:
                        currentValueIndex++;
                        if (currentValueIndex > CurrentSettings.DisplayedSettings[currentSettingIndex].AllowedValues.Count - 1)
                            currentValueIndex--;

                        break;
                    case ConsoleKey.LeftArrow:
                        currentValueIndex--;
                        if (currentValueIndex < 0)
                            currentValueIndex++;

                        break;
                }

                Render();
            }
        }
    }
}
*/
