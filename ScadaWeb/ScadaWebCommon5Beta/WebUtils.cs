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
 * Summary  : The class contains utility methods for web applications
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2016
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Scada.Web
{
    /// <summary>
    /// The class contains utility methods for web applications
    /// <para>Класс, содержащий вспомогательные методы для веб-приложений</para>
    /// </summary>
    public static class WebUtils
    {
        /// <summary>
        /// Проверить HTTP-контекст и его основные свойства на null
        /// </summary>
        public static void CheckHttpContext(HttpContext httpContext, bool checkCookies = false)
        {
            const string msg = "HTTP context or its properties are undefined.";

            if (httpContext == null)
                throw new ArgumentNullException("httpContext", msg);
            if (httpContext.Session == null)
                throw new ArgumentNullException("httpContext.Session", msg);
            if (httpContext.Request == null)
                throw new ArgumentNullException("httpContext.Request", msg);
            if (httpContext.Response == null)
                throw new ArgumentNullException("httpContext.Response", msg);

            if (checkCookies)
            {
                if (httpContext.Request.Cookies == null)
                    throw new ArgumentNullException("httpContext.Request.Cookies", msg);
                if (httpContext.Response.Cookies == null)
                    throw new ArgumentNullException("httpContext.Response.Cookies", msg);
            }
        }

        /// <summary>
        /// Отключить кэширование страницы
        /// </summary>
        public static void DisablePageCache(HttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            response.AppendHeader("Pragma", "No-cache");
            response.AppendHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        }

        /// <summary>
        /// Преобразовать строку для вывода на веб-страницу, заменив "\n" на тег "br"
        /// </summary>
        public static string HtmlEncodeWithBreak(string s)
        {
            return HttpUtility.HtmlEncode(s).Replace("\n", "<br />");
        }

        /// <summary>
        /// Преобразовать параметр запроса в массив целых чисел
        /// </summary>
        public static int[] QueryParamToIntArray(string param)
        {
            try
            {
                string[] elems = (param ?? "").Split(new char[] { ' ', ',' },
                    StringSplitOptions.RemoveEmptyEntries);
                int len = elems.Length;
                int[] arr = new int[len];

                for (int i = 0; i < len; i++)
                    arr[i] = int.Parse(elems[i]);

                return arr;
            }
            catch (FormatException ex)
            {
                throw new FormatException("Query parameter is not array of integers.", ex);
            }
        }

        /// <summary>
        /// Преобразовать словарь в объект JavaScript
        /// </summary>
        public static string DictionaryToJs(Localization.Dict dict)
        {
            StringBuilder sbJs = new StringBuilder();
            sbJs.AppendLine("{");

            if (dict != null)
            {
                foreach (KeyValuePair<string, string> pair in dict.Phrases)
                {
                    sbJs.Append(pair.Key).Append(": \"").Append(pair.Value).AppendLine("\",");
                }
            }

            sbJs.AppendLine("}");
            return sbJs.ToString();
        }
    }
}