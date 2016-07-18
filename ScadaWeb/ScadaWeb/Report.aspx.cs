/*
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
 * Module   : SCADA-Web
 * Summary  : Available reports web form
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2007
 * Modified : 2016
 */

using Scada.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.UI.WebControls;
using Utils;
using Utils.Report;

namespace Scada.Web
{
    /// <summary>
    /// Available reports web form
    /// <para>��?����?��������?������?/para>
    /// </summary>
    public partial class WFrmReport : System.Web.UI.Page
    {
        /// <summary>
        /// �������� ����??������ ������������ ������?        /// </summary>
        private void AddReport(int num, string text, string link)
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();

            cell.Text = num + ".";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Text = "<a href=\"report/" + link + "\">" + text + "</a>";
            row.Cells.Add(cell);

            row.Cells.Add(cell);
            tblReport.Rows.Add(row);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ��������?������ ����������?
            UserData userData = UserData.GetUserData();

            // �������� ����??������?            
            if (!userData.LoggedOn)
                throw new Exception(WebPhrases.NotLoggedOn);

            // ������?��?��������
            Translator.TranslatePage(this, "Scada.Web.WFrmReport");

            // ���������� ������ ������?            
            DirectoryInfo dirInfo = new DirectoryInfo(AppData.BinDir);
            SortedList<string, RepBuilder> repList = new SortedList<string, RepBuilder>();

            if (dirInfo.Exists)
            {
                FileInfo[] fileInfoArr = dirInfo.GetFiles("Rep*.dll", SearchOption.TopDirectoryOnly);
                foreach (FileInfo fileInfo in fileInfoArr)
                {
                    string fileName = fileInfo.Name;
                    string fullName = fileInfo.FullName;

                    // ������?���������� �������� ������������ ������ ?��������?������? �� ������?������������ ����
                    if (fileName == "RepBuilder.dll" || !userData.GetRight(fileName).ViewRight)
                        continue;

                    // �������� ���������� (������)
                    Assembly asm = null; // ����������
                    try
                    {
                        asm = Assembly.LoadFile(fullName);
                    }
                    catch (Exception ex)
                    {
                        AppData.Log.WriteAction(string.Format(Localization.UseRussian ? 
                            "������ ��?�������� ������ �� ����������\n{0}\n{1}" : 
                            "Error loading report from the assembly\n{0}\n{1}", fullName, ex.Message), 
                            Log.ActTypes.Error);
                        continue;
                    }

                    // ��������?���� �� ����������?����������
                    Type repType = null;
                    string typeName = "Scada.Report." + fileName.Substring(0, fileName.Length - 4);
                    string unableMsg = string.Format(Localization.UseRussian ?
                        "�� ������?�������� ��?������ {0} �� ����������\n{1}" :
                        "Unable to get the report type {0} from the assembly\n{1}", typeName, fullName);

                    try
                    {
                        repType = asm.GetType(typeName);
                        if (repType == null)
                        {
                            AppData.Log.WriteAction(unableMsg, Log.ActTypes.Error);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppData.Log.WriteAction(unableMsg + "\n" + ex.Message, Log.ActTypes.Error);
                        continue;
                    }

                    try
                    {
                        // �������� ������?�� ������ ������
                        RepBuilder rep = Activator.CreateInstance(repType) as RepBuilder;

                        // ���������� ������ ?������
                        repList.Add(rep.RepName, rep);
                    }
                    catch (Exception ex)
                    {
                        AppData.Log.WriteAction(string.Format(Localization.UseRussian ?
                            "������ ��?�������� ������?�� ������ ������ {0} �� ����������\n{1}\n{2}" :
                            "Error creating report class instance {0} from the assembly\n{1}\n{2}", 
                            repType, fullName, ex.Message), Log.ActTypes.Error);
                    }
                }
            }

            // ����?������ ������?�� ����?            
            if (repList.Count == 0)
            {
                lblReportList.Visible = false;
                lblNoReports.Visible = true;
            }
            else
            {
                for (int i = 0; i < repList.Count; i++)
                {
                    RepBuilder rep = repList.Values[i];
                    AddReport(i + 1, rep.RepName, rep.WebFormFileName);
                }
            }
        }
    }
}