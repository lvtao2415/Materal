﻿using Materal.WPFCommon;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Materal.WPFUI.CtrlTest
{
    public class SearchBoxViewModel : NotifyPropertyChanged
    {
        public List<UserModel> DataSource { get; set; } = new List<UserModel>();

        public UserModel SelectedData { get; set; }

        public SearchBoxViewModel()
        {
            for (var i = 0; i < 999999; i++)
            {
                DataSource.Add(new UserModel
                {
                    ID = Guid.NewGuid(),
                    Name = "云A" + DataSource.Count
                });
            }
        }
    }
}