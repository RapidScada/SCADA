/*
 * Copyright 2015 Mikhail Shiryaev
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
 * Summary  : The common application data
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2005
 * Modified : 2015
 */

using System;
using System.IO;
using System.Web;
using System.Windows.Forms;
using Scada.Client;
using Utils;
using System.Text;

namespace Scada.Web
{
	/// <summary>
    /// The common application data
    /// <para>����?������ ��������?</para>
	/// </summary>
	public static class AppData
	{
        /// <summary>
        /// ��� ����?������?��������? �� ��������?        /// </summary>
        public const string DefLogFileName = "ScadaWeb.log";


        private static string dictWriteTimeStr; // ����� ������ ?����?��������

        
        /// <summary>
        /// ����������?        /// </summary>
        static AppData()
		{
            dictWriteTimeStr = "";
            Inited = false;
            Log = new Log(Log.Formats.Full);
            LogFileName = DefLogFileName;
            MainData = new MainData();
            WebSettings = new WebSettings();
        }


        /// <summary>
        /// �������� ������?������������?����?������ ��������?
        /// </summary>
        public static bool Inited { get; private set; }
        
        /// <summary>
        /// �������� ������ ��������?
        /// </summary>
        public static Log Log { get; private set; }

        /// <summary>
        /// �������� ������ ��� ������ ?������?������?        /// </summary>
        public static MainData MainData { get; private set; }

        /// <summary>
        /// �������� ��������?��?��������?
        /// </summary>
        public static WebSettings WebSettings { get; private set; }


        /// <summary>
        /// �������� ���������� ��������?
        /// </summary>
        public static string AppDir { get; private set; }

        /// <summary>
        /// �������� ���������� ����������� ������
        /// </summary>
        public static string BinDir { get; private set; }

        /// <summary>
        /// �������� ���������� ������������
        /// </summary>
        public static string ConfigDir { get; private set; }

        /// <summary>
        /// �������� ���������� �������� �� ��������?�����
        /// </summary>
        public static string LangDir { get; private set; }

        /// <summary>
        /// �������� ���������� ��������
        /// </summary>
        public static string LogDir { get; private set; }

        /// <summary>
        /// �������� ��?���������� ��� ����?������?��������? (��?����������)
        /// </summary>
        /// <remarks>��� ����?������ ��������������� �� ������������?����?������ ��������?</remarks>
        public static string LogFileName { get; set; }

        /// <summary>
        /// �������� ���������� ��?������?������?        /// </summary>
        public static string ReportDir { get; private set; }

        /// <summary>
        /// �������� ���������� �������� ������?        /// </summary>
        public static string TemplateDir { get; private set; }


        /// <summary>
        /// �������� ��������?��?��������?
        /// </summary>
        private static void RefreshWebSettings()
        {
            string errMsg;
            if (!WebSettings.LoadFromFile(AppData.ConfigDir + WebSettings.DefFileName, out errMsg))
                Log.WriteAction(errMsg, Log.ActTypes.Error);
        }

        /// <summary>
        /// �������� ������? ���� ���� �������� ���������
        /// </summary>
        private static void RefreshDictionaries()
        {
            DateTime fileWriteTime1 = GetFileWriteTime(Localization.GetDictionaryFileName(LangDir, "ScadaData"));
            DateTime fileWriteTime2 = GetFileWriteTime(Localization.GetDictionaryFileName(LangDir, "ScadaWeb"));
            string writeTimeStr = fileWriteTime1.ToString() + fileWriteTime2.ToString();

            if (dictWriteTimeStr != writeTimeStr)
            {
                dictWriteTimeStr = writeTimeStr;
                string errMsg;

                if (Localization.LoadingRequired(LangDir, "ScadaData"))
                {
                    if (Localization.LoadDictionaries(LangDir, "ScadaData", out errMsg))
                        CommonPhrases.Init();
                    else
                        Log.WriteAction(errMsg, Log.ActTypes.Error);
                }

                if (Localization.LoadingRequired(LangDir, "ScadaWeb"))
                {
                    if (Localization.LoadDictionaries(LangDir, "ScadaWeb", out errMsg))
                        WebPhrases.Init();
                    else
                        Log.WriteAction(errMsg, Log.ActTypes.Error);
                }
            }
        }

        /// <summary>
        /// �������� ����� ��������?������ ?����
        /// </summary>
        private static DateTime GetFileWriteTime(string fileName)
        {
            try { return File.GetLastWriteTime(fileName); }
            catch { return DateTime.MinValue; }
        }


        /// <summary>
        /// ���������������� ����?������ ��������?
        /// </summary>
        public static void InitAppData()
        {
            if (!Inited)
            {
                Inited = true;

                // ����������?���������� ��������?
                if (HttpContext.Current == null)
                {
                    AppDir = ScadaUtils.NormalDir(Path.GetDirectoryName(Application.ExecutablePath));
                    BinDir = AppDir;
                }
                else
                {
                    AppDir = ScadaUtils.NormalDir(HttpContext.Current.Request.PhysicalApplicationPath);
                    BinDir = AppDir + "bin" + Path.DirectorySeparatorChar;
                }

                ConfigDir = AppDir + "config" + Path.DirectorySeparatorChar;
                LangDir = AppDir + "lang" + Path.DirectorySeparatorChar;
                LogDir = AppDir + "log" + Path.DirectorySeparatorChar;
                ReportDir = AppDir + "report" + Path.DirectorySeparatorChar;
                TemplateDir = AppDir + "templates" + Path.DirectorySeparatorChar;

                // ��������?������?��� ������ ?������?������?                MainData.SettingsFileName = ConfigDir + CommSettings.DefFileName;

                // ��������?������?��������?
                Log.FileName = LogDir + LogFileName;
                Log.Encoding = Encoding.UTF8;
                Log.WriteBreak();
                Log.WriteAction(Localization.UseRussian ? "������������� ����?������ ��������?" : 
                    "Initialize common application data", Log.ActTypes.Action);
            }

            // ���������� ��������
            RefreshDictionaries();

            // ���������� �������� ��?��������?
            RefreshWebSettings();
        }
    }
}