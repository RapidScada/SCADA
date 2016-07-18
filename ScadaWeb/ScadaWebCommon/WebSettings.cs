/*
 * Copyright 2014 Mikhail Shiryaev
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
 * Summary  : Web application settings
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2011
 * Modified : 2014
 */

using System;
using System.IO;
using System.Xml;

namespace Scada.Web
{
	/// <summary>
    /// Web application settings
    /// <para>��������?��?��������?</para>
	/// </summary>
	public class WebSettings
	{
        /// <summary>
        /// ��� ����?�������� ��?��������? �� ��������?		/// </summary>
		public const string DefFileName = "WebSettings.xml";

        private string lastFileName;  // ��������?��������������? ��� ����?��������
        private DateTime lastFileAge; // ����� ���������� ��������� ����?��������


        /// <summary>
        /// ����������?        /// </summary>
        public WebSettings()
		{
            lastFileName = "";
            lastFileAge = DateTime.MinValue;
            SetToDefault();
        }


		/// <summary>
		/// �������� ��?���������� ������?��������? ������, ?		/// </summary>
        public int SrezRefrFreq { get; set; }

        /// <summary>
        /// �������� ��?���������� ������?��������? ������?        /// </summary>
        public int EventRefrFreq { get; set; }

        /// <summary>
        /// �������� ��?���������� ���������� ������������ ������?        /// </summary>
        public int EventCnt { get; set; }

        /// <summary>
        /// �������� ��?���������� ������?��������� ������?������?�� ������������?�� ��������?        /// </summary>
        public bool EventFltr { get; set; }

        /// <summary>
        /// �������� ��?���������� ���������?����?������?������? ��?������?������ ������, ?        /// </summary>
        public int DiagBreak { get; set; }

        /// <summary>
        /// �������� ��?���������� ���������� ������ ��������?
        /// </summary>
        public bool CmdEnabled { get; set; }

        /// <summary>
        /// �������� ��?���������� ������?������?�������� ������ ��������?
        /// </summary>
        public bool SimpleCmd { get; set; }

        /// <summary>
        /// �������� ��?���������� ���������� ���������� ����������?, ��������??������?        /// </summary>
        public bool RemEnabled { get; set; }


        /// <summary>
        /// ���������� ������? �������� �� ��������?        /// </summary>
        private void SetToDefault()
        {
            SrezRefrFreq = 5;
            EventRefrFreq = 5;
            EventCnt = 20;
            EventFltr = true;
            DiagBreak = 90;
            CmdEnabled = true;
            SimpleCmd = false;
            RemEnabled = false;
        }


        /// <summary>
        /// ������?����?�������� ��?��������?
        /// </summary>
        public WebSettings Clone()
        {
            WebSettings webSettings = new WebSettings();
            webSettings.SrezRefrFreq = SrezRefrFreq;
            webSettings.EventCnt = EventCnt;
            webSettings.EventRefrFreq = EventRefrFreq;
            webSettings.EventFltr = EventFltr;
            webSettings.DiagBreak = DiagBreak;
            webSettings.CmdEnabled = CmdEnabled;
            return webSettings;
        }

        /// <summary>
        /// ��������?��������?��?��������? �� ����? ���� ���� ���������
		/// </summary>
		public bool LoadFromFile(string fileName, out string errMsg)
		{
            try
            {
                if (!File.Exists(fileName))
                {
                    // ��������?�������� �� ��������?                    SetToDefault();
                    // ����?��������?, ���� ���� �� ����������
                    throw new FileNotFoundException(string.Format(CommonPhrases.NamedFileNotFound, fileName));
                }

                DateTime fileAge = File.GetLastWriteTime(fileName);

                if (lastFileName != fileName || lastFileAge != fileAge)
                {
                    // ��������?�������� �� ��������?                    SetToDefault();

                    // �������� ��������
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(fileName);

                    XmlNodeList xmlNodeList = xmlDoc.DocumentElement.SelectNodes("Param");
                    foreach (XmlElement xmlElement in xmlNodeList)
                    {
                        string name = xmlElement.GetAttribute("name").Trim();
                        string nameL = name.ToLower();
                        string val = xmlElement.GetAttribute("value").ToLower();

                        try
                        {
                            if (nameL == "srezrefrfreq")
                                SrezRefrFreq = int.Parse(val);
                            else if (nameL == "eventrefrfreq")
                                EventRefrFreq = int.Parse(val);
                            else if (nameL == "eventcnt")
                                EventCnt = int.Parse(val);
                            else if (nameL == "eventfltr")
                                EventFltr = val == "true";
                            else if (nameL == "diagbreak")
                                DiagBreak = int.Parse(val);
                            else if (nameL == "cmdenabled")
                                CmdEnabled = val == "true";
                            else if (nameL == "simplecmd")
                                SimpleCmd = val == "true";
                            else if (nameL == "remenabled")
                                RemEnabled = val == "true";
                        }
                        catch
                        {
                            throw new Exception(string.Format(CommonPhrases.IncorrectXmlParamVal, name));
                        }
                    }

                    lastFileName = fileName;
                    lastFileAge = fileAge;
                }

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = WebPhrases.LoadWebSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// ��������?��������?��?��������? ?����?        /// </summary>
        public bool SaveToFile(string fileName, out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement rootElem = xmlDoc.CreateElement("WebSettings");
                xmlDoc.AppendChild(rootElem);

                rootElem.AppendParamElem("SrezRefrFreq", SrezRefrFreq, 
                    "Text1,Values refresh frequency, sec");
                rootElem.AppendParamElem("EventRefrFreq", EventRefrFreq,
                    "������?��������? ������? ?, Events refresh frequency, sec");
                rootElem.AppendParamElem("EventCnt", EventCnt, 
                    "���������� ������������ ������?, Display events count");
                rootElem.AppendParamElem("EventFltr", EventFltr,
                    "��������?������?������?�� ������������?�� ��������?, Set 'View' events filter by default");
                rootElem.AppendParamElem("DiagBreak", DiagBreak,
                    "Text5,Distance between points on the diagramm to make a break, sec");
                rootElem.AppendParamElem("CmdEnabled", CmdEnabled,
                    "���������� ������ ��������?Enable commands");
                rootElem.AppendParamElem("SimpleCmd", SimpleCmd,
                    "������� �������� ������ ��������?Simple commands sending");
                rootElem.AppendParamElem("RemEnabled", RemEnabled,
                    "���������� ���������� ����������?, ��������??������?Enable to remember logged on user");

                xmlDoc.Save(fileName);
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = WebPhrases.SaveWebSettingsError + ":\n" + ex.Message;
                return false;
            }
        }
    }
}