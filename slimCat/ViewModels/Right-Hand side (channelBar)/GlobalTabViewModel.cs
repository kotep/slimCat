﻿/*
Copyright (c) 2013, Justin Kadrovach
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL JUSTIN KADROVACH BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Models;
using Views;

namespace ViewModels
{
    /// <summary>
    /// On the channel bar (right-hand side) the 'users' tab, only it shows the entire list
    /// </summary>
    public class GlobalTabViewModel : ChannelbarViewModelCommon
    {
        #region Fields
        private GenderSettingsModel _genderSettings;
        public const string GlobalTabView = "GlobalTabView";
        private int _listCacheCount = -1;
        private const int _updateUsersTabResolution = 5 * 1000; // in ms, how often we check for a change in the users list
        private System.Timers.Timer _updateTick; // this fixes various issues with the users list not updating
        #endregion

        #region Properties
        public GenderSettingsModel GenderSettings { get { return _genderSettings; } }
        public GeneralChannelModel SelectedChan { get { return CM.SelectedChannel as GeneralChannelModel; } }

        public IEnumerable<ICharacter> SortedUsers
        {
            get
            {
                return CM.OnlineCharacters
                            .Where(MeetsFilter)
                            .OrderBy(RelationshipToUser)
                            .ThenBy(x => x.Name);
            }
        }

        public string SortContentString
        {
            get
            {
                return "Global";
            }
        }
        #endregion

        #region filter functions
        private bool MeetsFilter(ICharacter character)
        {
            return character.MeetsFilters(GenderSettings, SearchSettings, CM, null);
        }

        private string RelationshipToUser(ICharacter character)
        {
            return character.RelationshipToUser(CM, null);
        }
        #endregion

        #region Constructors
        public GlobalTabViewModel(IChatModel cm, IUnityContainer contain, IRegionManager regman, IEventAggregator eventagg)
            : base(contain, regman, eventagg, cm)
        {
            _container.RegisterType<object, GlobalTabView>(GlobalTabView);
            _genderSettings = new GenderSettingsModel();

            SearchSettings.Updated += (s, e) =>
            {
                OnPropertyChanged("SortedUsers");
                OnPropertyChanged("SearchSettings");
            };

            GenderSettings.Updated += (s, e) =>
            {
                OnPropertyChanged("GenderSettings");
                OnPropertyChanged("SortedUsers");
            };

            _events.GetEvent<slimCat.NewUpdateEvent>().Subscribe(
                args =>
                {
                    var thisNotification = args as CharacterUpdateModel;
                    if (thisNotification == null) return;

                    var thisArgument = thisNotification.Arguments as CharacterUpdateModel.ListChangedEventArgs;
                    if (thisArgument == null)
                        return;
                    else
                        OnPropertyChanged("SortedUsers");
                });

            _updateTick = new System.Timers.Timer(_updateUsersTabResolution);
            _updateTick.Elapsed += TickUpdateEvent;
            _updateTick.Start();
            _updateTick.AutoReset = true;
        }
        #endregion

        #region Methods
        private void TickUpdateEvent(object sender = null, EventArgs e = null)
        {
            if (SortedUsers != null)
            {
                try
                {
                    if (_listCacheCount != -1)
                    {
                        if (SortedUsers.Count() != _listCacheCount)
                        {
                            OnPropertyChanged("SortedUsers");
                            _listCacheCount = SortedUsers.Count();
                        }

                    }
                    else
                        _listCacheCount = SortedUsers.Count();
                }

                catch (InvalidOperationException) // if our collection changes while we're going through it, then it's obviously changed
                {
                    OnPropertyChanged("SortedUsers");
                }
            }
        }
        #endregion
    }
}