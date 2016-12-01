﻿/*
 * Copyright 2016 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaWebCommon
 * Summary  : The phrases used by the web application and its plugins
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2016
 */

#pragma warning disable 1591 // отключение warning CS1591: Missing XML comment for publicly visible type or member

namespace Scada.Web
{
    /// <summary>
    /// The phrases used by the web application and its plugins
    /// <para>Фразы, используемые веб-приложением и его плагинами</para>
    /// </summary>
    public static class WebPhrases
    {
        static WebPhrases()
        {
            SetToDefault();
        }

        // Словарь Scada.Web
        public static string NotLoggedOn { get; private set; }

        // Словарь Scada.Web.AppData
        public static string ServerUnavailable { get; private set; }
        public static string WrongPassword { get; private set; }
        public static string NoRights { get; private set; }
        public static string IllegalRole { get; private set; }

        // Словарь Scada.Web.WebSettings
        public static string LoadWebSettingsError { get; private set; }
        public static string SaveWebSettingsError { get; private set; }

        // Словарь Scada.Web.Shell.MenuItem
        public static string ReportsMenuItem { get; private set; }
        public static string ConfigMenuItem { get; private set; }
        public static string AboutMenuItem { get; private set; }

        // Словарь Scada.Web.Shell.RememberMe
        public static string SecurityViolation { get; private set; }

        private static void SetToDefault()
        {
            NotLoggedOn = Localization.Dict.GetEmptyPhrase("NotLoggedOn");

            ServerUnavailable = Localization.Dict.GetEmptyPhrase("ServerUnavailable");
            WrongPassword = Localization.Dict.GetEmptyPhrase("WrongPassword");
            NoRights = Localization.Dict.GetEmptyPhrase("NoRights");
            IllegalRole = Localization.Dict.GetEmptyPhrase("IllegalRole");

            LoadWebSettingsError = Localization.Dict.GetEmptyPhrase("LoadWebSettingsError");
            SaveWebSettingsError = Localization.Dict.GetEmptyPhrase("SaveWebSettingsError");

            ReportsMenuItem = Localization.Dict.GetEmptyPhrase("ReportsMenuItem");
            ConfigMenuItem = Localization.Dict.GetEmptyPhrase("ConfigMenuItem");
            AboutMenuItem = Localization.Dict.GetEmptyPhrase("AboutMenuItem");

            SecurityViolation = Localization.Dict.GetEmptyPhrase("SecurityViolation");
        }

        public static void Init()
        {
            Localization.Dict dict;
            if (Localization.Dictionaries.TryGetValue("Scada.Web", out dict))
            {
                NotLoggedOn = dict.GetPhrase("NotLoggedOn", NotLoggedOn);
            }

            if (Localization.Dictionaries.TryGetValue("Scada.Web.AppData", out dict))
            {
                ServerUnavailable = dict.GetPhrase("ServerUnavailable", ServerUnavailable);
                WrongPassword = dict.GetPhrase("WrongPassword", WrongPassword);
                NoRights = dict.GetPhrase("NoRightsL", NoRights);
                IllegalRole = dict.GetPhrase("IllegalRole", IllegalRole);
            }

            if (Localization.Dictionaries.TryGetValue("Scada.Web.WebSettings", out dict))
            {
                LoadWebSettingsError = dict.GetPhrase("LoadWebSettingsError", LoadWebSettingsError);
                SaveWebSettingsError = dict.GetPhrase("SaveWebSettingsError", SaveWebSettingsError);
            }

            if (Localization.Dictionaries.TryGetValue("Scada.Web.Shell.MenuItem", out dict))
            {
                ReportsMenuItem = dict.GetPhrase("ReportsMenuItem", ReportsMenuItem);
                ConfigMenuItem = dict.GetPhrase("ConfigMenuItem", ConfigMenuItem);
                AboutMenuItem = dict.GetPhrase("AboutMenuItem", AboutMenuItem);
            }

            if (Localization.Dictionaries.TryGetValue("Scada.Web.Shell.RememberMe", out dict))
            {
                SecurityViolation = dict.GetPhrase("SecurityViolation", SecurityViolation);
            }
        }
    }
}
