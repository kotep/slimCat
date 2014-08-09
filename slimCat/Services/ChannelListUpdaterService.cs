﻿#region Copyright

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TicketProvider.cs">
//    Copyright (c) 2013, Justin Kadrovach, All rights reserved.
//   
//    This source is subject to the Simplified BSD License.
//    Please see the License.txt file for more information.
//    All other rights reserved.
//    
//    THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//    PARTICULAR PURPOSE.
// </copyright>
//  --------------------------------------------------------------------------------------------------------------------

#endregion

namespace slimCat.Services
{
    #region Usings
    using System;
    using System.Timers;
    using Microsoft.Practices.Prism.Events;
    using Utilities;

    #endregion

    public class ChannelListUpdaterService : IChannelListUpdater
    {
        private readonly IChatConnection connection;

        private DateTime lastUpdate;

        public ChannelListUpdaterService(IChatConnection connection, IEventAggregator eventAggregator)
        {
            this.connection = connection;

            eventAggregator.GetEvent<ConnectionClosedEvent>().Subscribe(OnWipeState);

            var timer = new Timer(60*1000*1);
            timer.Elapsed += (s, e) =>
            {
                UpdateChannels();
                timer.Dispose();
            };
            timer.Start();
        }

        private void OnWipeState(string o)
        {
            lastUpdate = new DateTime();
        }

        public void UpdateChannels()
        {
            if (lastUpdate.AddHours(2) > DateTime.Now) return;

            connection.SendMessage(Constants.ClientCommands.PublicChannelList);
            connection.SendMessage(Constants.ClientCommands.PrivateChannelList);
            lastUpdate = DateTime.Now;
        }

    }

    public interface IChannelListUpdater
    {
        void UpdateChannels();
    }
}
