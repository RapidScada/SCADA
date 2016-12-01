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
 * Module   : PlgSchemeCommon
 * Summary  : WCF service for interacting with the scheme JavaScript code
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2016
 */

using Scada.Scheme;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;

namespace Scada.Web.Plugins.Scheme
{
    /// <summary>
    /// WCF service for interacting with the scheme JavaScript code
    /// <para>WCF-сервис для взаимодействия с JavaScript-кодом схемы</para>
    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SchemeSvc
    {
        /// <summary>
        /// Базовый класс объекта для передачи схемы
        /// </summary>
        private abstract class SchemeDTO
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public SchemeDTO()
            {
                ViewStamp = 0;
            }

            /// <summary>
            /// Получить или установить метку представления, уникальную в пределах приложения
            /// </summary>
            public long ViewStamp { get; set; }
        }

        /// <summary>
        /// Класс объекта для передачи свойств схемы
        /// </summary>
        private class SchemePropsDTO : SchemeDTO
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public SchemePropsDTO()
                : base()
            {
                SchemeProps = null;
                ElementCount = 0;
                ImageCount = 0;
            }

            /// <summary>
            /// Получить или установить свойства схемы
            /// </summary>
            public SchemeView.Scheme SchemeProps { get; set; }
            /// <summary>
            /// Получить или установить количество элементов схемы
            /// </summary>
            public int ElementCount { get; set; }
            /// <summary>
            /// Получить или установить количество изображений схемы
            /// </summary>
            public int ImageCount { get; set; }
        }

        /// <summary>
        /// Класс объекта для передачи элементов схемы
        /// </summary>
        private class ElementsDTO : SchemeDTO
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ElementsDTO(int capacity = -1)
                : base()
            {
                EndOfElements = false;
                Elements = capacity > 0 ? 
                    new List<SchemeView.Element>(capacity) : 
                    new List<SchemeView.Element>();
            }

            /// <summary>
            /// Получить или установить признак, что считаны все элементы схемы
            /// </summary>
            public bool EndOfElements { get; set; }
            /// <summary>
            /// Получить элементы схемы
            /// </summary>
            public List<SchemeView.Element> Elements { get; private set; }
        }

        /// <summary>
        /// Класс объекта для передачи изображения
        /// </summary>
        private class ImageDTO
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ImageDTO(SchemeView.Image image)
            {
                Name = image.Name ?? "";
                Data = Convert.ToBase64String(image.Data == null ? new byte[0] : image.Data, 
                    Base64FormattingOptions.None);
                SetMediaType();
            }

            /// <summary>
            /// Получить наименование
            /// </summary>
            public string Name { get; private set; }
            /// <summary>
            /// Получить медиа-тип
            /// </summary>
            public string MediaType { get; private set; }
            /// <summary>
            /// Получить данные в формате base 64
            /// </summary>
            public string Data { get; private set; }

            /// <summary>
            /// Установить медиа-тип на основе наименования
            /// </summary>
            private void SetMediaType()
            {
                string ext = Path.GetExtension(Name).ToLowerInvariant();
                if (ext == ".png")
                    MediaType = "image/png";
                else if (ext == ".jpg")
                    MediaType = "image/jpeg";
                else if (ext == ".gif")
                    MediaType = "image/gif";
                else if (ext == ".svg")
                    MediaType = "image/svg+xml";
                else
                    MediaType = "";
            }
        }

        /// <summary>
        /// Класс объекта для передачи изображений схемы
        /// </summary>
        private class ImagesDTO : SchemeDTO
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ImagesDTO()
                : base()
            {
                EndOfImages = false;
                Images = new List<ImageDTO>();
            }

            /// <summary>
            /// Получить или установить признак, что считаны все изображения схемы
            /// </summary>
            public bool EndOfImages { get; set; }
            /// <summary>
            /// Получить изображения схемы
            /// </summary>
            public List<ImageDTO> Images { get; private set; }
        }


        /// <summary>
        /// Мексимальное количество символов строке данных в формате JSON, 10 МБ
        /// </summary>
        private const int MaxJsonLen = 10485760;
        /// <summary>
        /// Обеспечивает сериализацию результатов методов сервиса
        /// </summary>
        private static readonly JavaScriptSerializer JsSerializer = new JavaScriptSerializer() { MaxJsonLength = MaxJsonLen };
        /// <summary>
        /// Общие данные веб-приложения
        /// </summary>
        private static readonly AppData AppData = AppData.GetAppData();


        /// <summary>
        /// Получить свойства схемы
        /// </summary>
        [OperationContract]
        [WebGet]
        public string GetSchemeProps(int viewID, long viewStamp)
        {
            try
            {
                AppData.CheckLoggedOn();
                SchemeView schemeView = AppData.ViewCache.GetView<SchemeView>(viewID, true);
                SchemePropsDTO dto = new SchemePropsDTO();
                dto.ViewStamp = schemeView.Stamp;

                if (viewStamp == 0 || viewStamp == dto.ViewStamp)
                {
                    // копирование свойств схемы без словаря изображений
                    SchemeView.Scheme srcSchemeProps = schemeView.SchemeParams;
                    dto.SchemeProps = new SchemeView.Scheme(null, schemeView.CnlsFilter)
                    {
                        Size = srcSchemeProps.Size,
                        BackColor = srcSchemeProps.BackColor,
                        BackImage = srcSchemeProps.BackImage,
                        ForeColor = srcSchemeProps.ForeColor,
                        Font = srcSchemeProps.Font,
                        Title = srcSchemeProps.Title
                    };

                    dto.ElementCount = schemeView.ElementList.Count;
                    dto.ImageCount = schemeView.ImageDict.Count;
                }

                return JsSerializer.Serialize(dto);
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении свойств схемы с ид.={0}" :
                    "Error getting the properties of the scheme with ID={0}", viewID);
                return "";
            }
        }

        /// <summary>
        /// Получить элементы схемы
        /// </summary>
        [OperationContract]
        [WebGet]
        public string GetElements(int viewID, long viewStamp, int startIndex, int count)
        {
            try
            {
                AppData.CheckLoggedOn();
                SchemeView schemeView = AppData.ViewCache.GetView<SchemeView>(viewID, true);
                ElementsDTO dto = new ElementsDTO(count);
                dto.ViewStamp = schemeView.Stamp;

                if (viewStamp == 0 || viewStamp == dto.ViewStamp)
                {
                    List<SchemeView.Element> srcElems = schemeView.ElementList;
                    int srcCnt = srcElems.Count;
                    dto.EndOfElements = startIndex + count >= srcCnt;

                    for (int i = startIndex, j = 0; i < srcCnt && j < count; i++, j++)
                        dto.Elements.Add(srcElems[i]);
                }

                return JsSerializer.Serialize(dto);
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении элементов схемы с ид.={0}" :
                    "Error getting the elements of the scheme with ID={0}", viewID);
                return "";
            }
        }

        /// <summary>
        /// Получить изображения схемы
        /// </summary>
        [OperationContract]
        [WebGet]
        public string GetImages(int viewID, long viewStamp, int startIndex, int totalDataSize)
        {
            try
            {
                AppData.CheckLoggedOn();
                SchemeView schemeView = AppData.ViewCache.GetView<SchemeView>(viewID, true);
                ImagesDTO dto = new ImagesDTO();
                dto.ViewStamp = schemeView.Stamp;

                if (viewStamp == 0 || viewStamp == dto.ViewStamp)
                {
                    Dictionary<string, SchemeView.Image>.ValueCollection images = schemeView.ImageDict.Values;
                    int i = 0;
                    int size = 0;

                    foreach (SchemeView.Image image in images)
                    {
                        if (i >= startIndex)
                        {
                            dto.Images.Add(new ImageDTO(image));
                            if (image.Data != null)
                                size += image.Data.Length;
                        }

                        if (size >= totalDataSize)
                            break;

                        i++;
                    }

                    dto.EndOfImages = i == images.Count;
                }

                return JsSerializer.Serialize(dto);
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении изображений схемы с ид.={0}" :
                    "Error getting the images of the scheme with ID={0}", viewID);
                return "";
            }
        }
    }
}
