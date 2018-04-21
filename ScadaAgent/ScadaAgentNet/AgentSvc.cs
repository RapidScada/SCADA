﻿/*
 * Copyright 2018 Mikhail Shiryaev
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
 * Module   : ScadaAgentNet
 * Summary  : WCF service for interacting with the agent
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2018
 * Modified : 2018
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Utils;

namespace Scada.Agent.Net
{
    /// <summary>
    /// WCF service for interacting with the agent
    /// <para>WCF-сервис для взаимодействия с агентом</para>
    /// </summary>
    [ServiceContract]
    public class AgentSvc
    {
        /// <summary>
        /// Размер буфера для приёма файлов
        /// </summary>
        private int ReceiveBufSize = 1024;

        /// <summary>
        /// Данные приложения
        /// </summary>
        private static readonly AppData AppData = AppData.GetInstance();
        /// <summary>
        /// Журнал приложения
        /// </summary>
        private static readonly ILog Log = AppData.Log;
        /// <summary>
        /// Менеджер сессий
        /// </summary>
        private static readonly SessionManager SessionManager = AppData.SessionManager;
        /// <summary>
        /// Менеджер экземпляров систем
        /// </summary>
        private static readonly InstanceManager InstanceManager = AppData.InstanceManager;


        /// <summary>
        /// Получить IP-адрес текущего подключения
        /// </summary>
        private string GetClientIP()
        {
            try
            {
                OperationContext context = OperationContext.Current;
                MessageProperties props = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty remoteEndPoint =
                       (RemoteEndpointMessageProperty)props[RemoteEndpointMessageProperty.Name];
                return remoteEndPoint.Address;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Попытаться получить сессию по идентификатору
        /// </summary>
        private bool TryGetSession(long sessionID, out Session session)
        {
            session = SessionManager.GetSession(sessionID);

            if (session == null)
            {
                Log.WriteError(string.Format(Localization.UseRussian ?
                    "Сессия с ид. {0} не найдена" :
                    "Session with ID {0} not found", sessionID));
                return false;
            }
            else
            {
                session.RegisterActivity();
                return true;
            }
        }

        /// <summary>
        /// Попытаться получить экземпляр системы по ид. сессии
        /// </summary>
        private bool TryGetScadaInstance(long sessionID, out ScadaInstance scadaInstance)
        {
            if (TryGetSession(sessionID, out Session session))
            {
                scadaInstance = session.LoggedOn ? session.ScadaInstance : null;

                if (scadaInstance == null)
                {
                    Log.WriteError(string.Format(Localization.UseRussian ?
                        "Экземпляр системы не определён для сессии с ид. {0}" :
                        "System instance is not defined for a session with ID {0}", sessionID));
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                scadaInstance = null;
                return false;
            }
        }

        /// <summary>
        /// Проверить сообщение для загрузки конфигурации
        /// </summary>
        private bool ValidateMessage(ConfigUploadMessage message)
        {
            return message != null && message.ConfigOptions != null || message.Stream != null;
        }

        /// <summary>
        /// Принять файл
        /// </summary>
        private bool ReceiveFile(Stream srcStream, string destFileName)
        {
            try
            {
                DateTime t0 = DateTime.UtcNow;
                byte[] buffer = new byte[ReceiveBufSize];

                using (FileStream destStream = File.Create(destFileName))
                {
                    int readCnt;
                    while ((readCnt = srcStream.Read(buffer, 0, ReceiveBufSize)) > 0)
                    {
                        destStream.Write(buffer, 0, readCnt);
                    }
                }

                Log.WriteAction(string.Format(Localization.UseRussian ?
                    "Файл {0} принят успешно за {1} мс" :
                    "File {0} received successfully in {1} ms", 
                    Path.GetFileName(destFileName), (int)(DateTime.UtcNow - t0).TotalMilliseconds));
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при приёме файла" :
                    "Error receiving file");
                return false;
            }
        }


        /// <summary>
        /// Создать новую сессию
        /// </summary>
        [OperationContract]
        public bool CreateSession(out long sessionID)
        {
            Session session = SessionManager.CreateSession();

            if (session == null)
            {
                sessionID = 0;
                return false;
            }
            else
            {
                session.IpAddress = GetClientIP();
                sessionID = session.ID;
                return true;
            }
        }

        /// <summary>
        /// Войти в систему
        /// </summary>
        [OperationContract]
        public bool Login(long sessionID, string username, string encryptedPassword, string scadaInstanceName)
        {
            if (TryGetSession(sessionID, out Session session))
            {
                session.ClearUser();
                ScadaInstance scadaInstance = InstanceManager.GetScadaInstance(scadaInstanceName);

                if (scadaInstance == null)
                {
                    Log.WriteError(string.Format(Localization.UseRussian ?
                        "Экземпляр системы с наименованием \"{0}\" не найден" :
                        "System instance named \"{0}\" not found", scadaInstanceName));
                }
                else if (scadaInstance.ValidateUser(username, encryptedPassword, out string errMsg))
                {
                    session.SetUser(username, scadaInstance);
                    return true;
                }
                else
                {
                    Log.WriteError(string.Format(Localization.UseRussian ?
                        "Пользователь {0} не прошёл проверку - {1}" :
                        "User {0} failed validation - {1}", username, errMsg));
                }
            }

            return false;
        }

        /// <summary>
        /// Управлять службой
        /// </summary>
        [OperationContract]
        public bool ControlService(long sessionID, ServiceApp serviceApp, ServiceCommand command)
        {
            return true;
        }

        /// <summary>
        /// Получить статус службы
        /// </summary>
        [OperationContract]
        public bool GetServiceStatus(long sessionID, ServiceApp serviceApp, out bool isRunning)
        {
            isRunning = true;
            return true;
        }

        /// <summary>
        /// Получить доступные части конфигурации экземпляра системы
        /// </summary>
        [OperationContract]
        public bool GetAvailableConfig(long sessionID, out ConfigParts configParts)
        {
            configParts = ConfigParts.All;
            return true;
        }

        /// <summary>
        /// Скачать конфигурацию
        /// </summary>
        [OperationContract]
        public Stream DownloadConfig(long sessionID, ConfigOptions configOptions)
        {
            if (TryGetScadaInstance(sessionID, out ScadaInstance scadaInstance))
            {
                lock (scadaInstance.SyncRoot)
                {
                    string tempFileName = AppData.GetTempFileName("download-config", "zip");
                    if (scadaInstance.PackConfig(tempFileName, configOptions))
                    {
                        return File.Open(tempFileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Загрузить конфигурацию
        /// </summary>
        [OperationContract]
        public void UploadConfig(ConfigUploadMessage configUploadMessage)
        {
            if (ValidateMessage(configUploadMessage))
            {
                if (TryGetScadaInstance(configUploadMessage.SessionID, out ScadaInstance scadaInstance))
                {
                    lock (scadaInstance.SyncRoot)
                    {
                        string tempFileName = AppData.GetTempFileName("upload-config", "zip");
                        if (ReceiveFile(configUploadMessage.Stream, tempFileName))
                        {
                            scadaInstance.UnpackConfig(tempFileName, configUploadMessage.ConfigOptions);
                        }
                    }
                }
            }
            else 
            {
                Log.WriteError(Localization.UseRussian ?
                    "Загружаемая конфигурация не определена или некорректна" :
                    "Uploaded configuration is undefined or incorrect");
            }
        }

        /// <summary>
        /// Найти файлы
        /// </summary>
        [OperationContract]
        public bool FindFiles(long sessionID, RelPath relPath, out ICollection<string> paths)
        {
            paths = null;
            return true;
        }

        /// <summary>
        /// Скачать файл
        /// </summary>
        [OperationContract]
        public Stream DownloadFile(long sessionID, RelPath relPath)
        {
            /*byte[] buffer = System.Text.Encoding.ASCII.GetBytes("hello");
            MemoryStream stream = new MemoryStream(buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;*/

            Stream stream = File.Open("big.txt", FileMode.Open);
            return stream;
        }

        /// <summary>
        /// Скачать часть файла с заданной позиции
        /// </summary>
        [OperationContract]
        public Stream DownloadFileRest(long sessionID, RelPath relPath, long position)
        {
            return null;
        }
    }
}
