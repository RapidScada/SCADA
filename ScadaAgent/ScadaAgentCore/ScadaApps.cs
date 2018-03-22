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
 * Module   : ScadaAgentCore
 * Summary  : Applications
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2018
 * Modified : 2018
 */

using System;

namespace Scada.Agent
{
    /// <summary>
    /// Applications
    /// <para>Приложения</para>
    /// </summary>
    [Flags]
    public enum ScadaApps
    {
        /// <summary>
        /// Не задано
        /// </summary>
        None = 0,

        /// <summary>
        /// Сервер
        /// </summary>
        Server = 1,

        /// <summary>
        /// Коммуникатор
        /// </summary>
        Communicator = 2,

        /// <summary>
        /// Вебстанция
        /// </summary>
        Webstation = 4
    }
}
