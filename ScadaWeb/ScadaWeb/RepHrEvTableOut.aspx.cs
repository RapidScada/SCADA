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
 * Module   : SCADA-Web
 * Summary  : Output page of "Hourly data and events" report
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2007
 * Modified : 2014
 */

using System;
using System.IO;
using Scada.Client;
using Utils;
using Utils.Report;

namespace Scada.Web
{
    /// <summary>
    /// Output page of "Hourly data and events" report
    /// <para>�������� �������� ������ "������� ����� � �������"</para>
    /// </summary>
    public partial class WFrmRepHrEvTableOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ���������� ����������� ��������
            ScadaUtils.DisablePageCache(Response);

            // ��������� ������ ������������
            UserData userData = UserData.GetUserData();

            // �������� ����� � �������
            if (!userData.LoggedOn)
                throw new Exception(WebPhrases.NotLoggedOn);

            // ����������� �������� ���������� �������������
            int viewSetIndex, viewIndex;
            if (!int.TryParse(Request["viewSet"], out viewSetIndex))
                viewSetIndex = -1;
            if (!int.TryParse(Request["view"], out viewIndex))
                viewIndex = -1;

            // ��������� ������������� � ���� ������������ �� ����
            BaseView baseView;
            MainData.Right right;
            TableView tableView = userData.GetView(null, viewSetIndex, viewIndex, out baseView, out right) ?
                baseView as TableView : null;

            // ����������� ���� ������ �������
            int eventOut;
            string eventOutStr = Request["eventOut"];

            if (eventOutStr == "all") 
                eventOut = 1; // ��� �������
            else if (eventOutStr == "view") 
                eventOut = 2; // �� �������������
            else 
                eventOut = 0; // �� ��������

            // �������� ���������� ��������� ������
            if (tableView == null && eventOut == 0)
                throw new Exception(WebPhrases.NoReportData);

            // �������� �������� ������������� � ���� �� ��������� ����������
            if (baseView == null)
                throw new Exception(WebPhrases.UnableLoadView);
            else if (!right.ViewRight || eventOut == 1 && userData.Role == ServerComm.Roles.Custom)
                throw new Exception(CommonPhrases.NoRights);

            // ����������� ����, �� ������� ����������� �����
            int year, month, day;
            int.TryParse(Request["year"], out year);
            int.TryParse(Request["month"], out month);
            int.TryParse(Request["day"], out day);

            DateTime reqDate;
            try
            {
                reqDate = new DateTime(year, month, day);
            }
            catch
            {
                throw new Exception(WebPhrases.IncorrectDate);
            }

            // �������� ������
            RepBuilder rep = new RepHrEvTable();

            try
            {
                // ����� � ������
                AppData.Log.WriteAction(string.Format(WebPhrases.GenReport, rep.RepName, userData.UserLogin), 
                    Log.ActTypes.Action);

                // ��������� ���� �������� � ����� ����� ������                
                Response.ClearHeaders();
                Response.ContentType = "application/octet-stream";
                Response.AppendHeader("Content-Disposition", "attachment;filename=\"" +
                    Path.GetFileNameWithoutExtension(baseView.ItfObjName) + reqDate.ToString(" yyyy-MM-dd") + ".xml\"");

                // ��������� ���������� ������
                rep.SetParams(baseView, reqDate, eventOut);

                // ��������� ������
                rep.Make(Response.OutputStream, Request.PhysicalApplicationPath + @"templates\");
            }
            catch (Exception ex)
            {
                string errMsg = string.Format(WebPhrases.GenReportError, rep.RepName, ex.Message);
                AppData.Log.WriteAction(errMsg, Log.ActTypes.Exception);

                Response.ClearHeaders();
                Response.ContentType = "text/html";
                Response.Write(ScadaUtils.HtmlEncodeWithBreak(errMsg));
            }
        }
    }
}