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
 * Module   : ScadaData
 * Summary  : Thread safe access to the client cache data
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2016
 */

using Scada.Data;
using System;
using System.Data;
using System.IO;
using Utils;

namespace Scada.Client
{
    /// <summary>
    /// Thread safe access to the client cache data
    /// <para>Потокобезопасный доступ к данным кеша клиентов</para>
    /// </summary>
    /// <remarks>The class replaces Scada.Web.MainData
    /// <para>Класс заменяет Scada.Web.MainData</para></remarks>
    public class DataAccess
    {
        /// <summary>
        /// Кеш данных
        /// </summary>
        protected readonly DataCache dataCache;
        /// <summary>
        /// Журнал
        /// </summary>
        protected readonly Log log;

        /// <summary>
        /// Объект для синхронизации доступа к таблицам базы конфигурации
        /// </summary>
        protected readonly object baseLock;
        /// <summary>
        /// Объект для синхронизации достапа к свойствам входных каналов
        /// </summary>
        protected readonly object cnlPropsLock;
        /// <summary>
        /// Объект для синхронизации достапа к свойствам каналов управления
        /// </summary>
        protected readonly object ctrlCnlPropsLock;
        /// <summary>
        /// Объект для синхронизации достапа к текущим даным
        /// </summary>
        protected readonly object curDataLock;


        /// <summary>
        /// Конструктор, ограничивающий создание объекта без параметров
        /// </summary>
        protected DataAccess()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DataAccess(DataCache dataCache, Log log)
        {
            if (dataCache == null)
                throw new ArgumentNullException("dataCache");
            if (log == null)
                throw new ArgumentNullException("log");

            this.dataCache = dataCache;
            this.log = log;

            baseLock = new object();
            cnlPropsLock = new object();
            ctrlCnlPropsLock = new object();
            curDataLock = new object();
        }


        /// <summary>
        /// Получить кеш данных
        /// </summary>
        public DataCache DataCache
        {
            get
            {
                return dataCache;
            }
        }


        /// <summary>
        /// Получить наименование роли по идентификатору из базы конфигурации
        /// </summary>
        protected string GetRoleNameFromBase(int roleID, string defaultRoleName)
        {
            lock (baseLock)
            {
                try
                {
                    dataCache.RefreshBaseTables();

                    DataTable tblRole = dataCache.BaseTables.RightTable;
                    BaseTables.CheckIsNotEmpty(tblRole, true);
                    tblRole.DefaultView.RowFilter = "RoleID = " + roleID;

                    return tblRole.DefaultView.Count > 0 ?
                        (string)tblRole.DefaultView[0]["Name"] :
                        defaultRoleName;
                }
                catch (Exception ex)
                {
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при получении наименования роли по идентификатору {0}" :
                        "Error getting role name by ID {0}", roleID);
                    return defaultRoleName;
                }
            }
        }


        /// <summary>
        /// Получить свойства входного канала по его номеру
        /// </summary>
        public InCnlProps GetCnlProps(int cnlNum)
        {
            try
            {
                lock (baseLock)
                {
                    dataCache.RefreshBaseTables();
                }

                lock (cnlPropsLock)
                {
                    // сохранение ссылки на свойства каналов,
                    // т.к. свойство CnlProps может быть изменено из другого потока
                    InCnlProps[] cnlProps = dataCache.CnlProps;

                    // поиск свойств заданного канала
                    int ind = Array.BinarySearch(cnlProps, cnlNum, InCnlProps.IntComp);
                    return ind >= 0 ? cnlProps[ind] : null;
                }
            }
            catch (Exception ex)
            {
                log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении свойств входного канала {0}" :
                    "Error getting input channel {0} properties", cnlNum);
                return null;
            }
        }

        /// <summary>
        /// Получить свойства канала управления по его номеру
        /// </summary>
        public CtrlCnlProps GetCtrlCnlProps(int ctrlCnlNum)
        {
            try
            {
                lock (baseLock)
                {
                    dataCache.RefreshBaseTables();
                }

                lock (ctrlCnlPropsLock)
                {
                    // сохранение ссылки на свойства каналов,
                    // т.к. свойство CtrlCnlProps может быть изменено из другого потока
                    CtrlCnlProps[] ctrlCnlProps = dataCache.CtrlCnlProps;

                    // поиск свойств заданного канала
                    int ind = Array.BinarySearch(ctrlCnlProps, ctrlCnlNum, CtrlCnlProps.IntComp);
                    return ind >= 0 ? ctrlCnlProps[ind] : null;
                }
            }
            catch (Exception ex)
            {
                log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении свойств канала управления {0}" :
                    "Error getting output channel {0} properties", ctrlCnlNum);
                return null;
            }
        }

        /// <summary>
        /// Получить свойства представления по идентификатору
        /// </summary>
        /// <remarks>Используется таблица объектов интерфейса</remarks>
        public ViewProps GetViewProps(int viewID)
        {
            lock (baseLock)
            {
                try
                {
                    dataCache.RefreshBaseTables();

                    DataTable tblInterface = dataCache.BaseTables.InterfaceTable;
                    BaseTables.CheckIsNotEmpty(tblInterface, true);
                    tblInterface.DefaultView.RowFilter = "ItfID = " + viewID;

                    if (tblInterface.DefaultView.Count > 0)
                    {
                        ViewProps viewProps = new ViewProps(viewID);
                        viewProps.FileName = (string)tblInterface.DefaultView[0]["Name"];
                        viewProps.ViewTypeCode = Path.GetExtension(viewProps.FileName);
                        return viewProps;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при получении свойств представления по ид.={0}" :
                        "Error getting view properties by ID={0}", viewID);
                    return null;
                }
            }
        }

        /// <summary>
        /// Получить идентификатор пользователя по имени
        /// </summary>
        public int GetUserID(string username)
        {
            lock (baseLock)
            {
                try
                {
                    username = username ?? "";
                    dataCache.RefreshBaseTables();

                    DataTable tblUser = dataCache.BaseTables.UserTable;
                    BaseTables.CheckIsNotEmpty(tblUser, true);
                    tblUser.DefaultView.RowFilter = "Name = '" + username + "'";

                    return tblUser.DefaultView.Count > 0 ?
                        (int)tblUser.DefaultView[0]["UserID"] :
                        BaseValues.EmptyDataID;
                }
                catch (Exception ex)
                {
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при получении идентификатора пользователя по имени \"{0}\"" :
                        "Error getting user ID by name \"{0}\"", username);
                    return BaseValues.EmptyDataID;
                }
            }
        }

        /// <summary>
        /// Получить наименование роли по идентификатору
        /// </summary>
        public string GetRoleName(int roleID)
        {
            string roleName = BaseValues.Roles.GetRoleName(roleID); // стандартное имя роли
            return BaseValues.Roles.Custom <= roleID && roleID < BaseValues.Roles.Err ?
                GetRoleNameFromBase(roleID, roleName) :
                roleName;
        }
        
        /// <summary>
        /// Получить цвет по статусу
        /// </summary>
        public string GetColorByStat(int stat, string defaultColor)
        {
            lock (baseLock)
            {
                try
                {
                    dataCache.RefreshBaseTables();

                    DataTable tblEvType = dataCache.BaseTables.EvTypeTable;
                    BaseTables.CheckIsNotEmpty(tblEvType, true);
                    tblEvType.DefaultView.RowFilter = "CnlStatus = " + stat;

                    if (tblEvType.DefaultView.Count > 0)
                    {
                        object colorObj = tblEvType.DefaultView[0]["Color"];
                        if (colorObj != DBNull.Value)
                            return colorObj.ToString();
                    }
                }
                catch (Exception ex)
                {
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при получении цвета по статусу {0}" :
                        "Error getting color by status {0}", stat);
                }

                return defaultColor;
            }
        }


        /// <summary>
        /// Получить текущие данные входного канала
        /// </summary>
        public SrezTableLight.CnlData GetCurCnlData(int cnlNum)
        {
            DateTime dateTime;
            return GetCurCnlData(cnlNum, out dateTime);
        }

        /// <summary>
        /// Получить текущие данные входного канала
        /// </summary>
        public SrezTableLight.CnlData GetCurCnlData(int cnlNum, out DateTime dataAge)
        {
            lock (curDataLock)
            {
                try
                {
                    SrezTableLight.Srez snapshot = dataCache.GetCurSnapshot(out dataAge);
                    SrezTableLight.CnlData cnlData;
                    return snapshot != null && snapshot.GetCnlData(cnlNum, out cnlData) ? 
                        cnlData : SrezTableLight.CnlData.Empty;
                }
                catch (Exception ex)
                {
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при получении текущих данных входного канала {0}" :
                        "Error getting current data of the input channel {0}", cnlNum);

                    dataAge = DateTime.MinValue;
                    return SrezTableLight.CnlData.Empty;
                }
            }
        }
    }
}
