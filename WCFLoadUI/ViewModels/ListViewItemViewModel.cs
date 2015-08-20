#region File Information/History
// <copyright file="ListViewItemViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.Collections.Generic;
using WCFLoad.Helper;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    public class ListViewItemViewModel : BaseViewModel
    {
        #region private fields
        private ControlView _properties;
        private string _selectedSpecialValue = string.Empty;
        #endregion

        #region constructors
        public ListViewItemViewModel(ControlView properties)
        {
            Properties = properties;
        }
        #endregion

        #region public properties
        public string SelectedSpecialValue
        {
            get
            {
                return _selectedSpecialValue;
            }
            set
            {
                _selectedSpecialValue = value;
                NotifyOfPropertyChange(() => SelectedSpecialValue);
            }
        }


        public ControlView Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
                NotifyOfPropertyChange(() => Properties);
            }
        }

        public List<string> SpecialValueList
        {
            get
            {
                return SpecialValue.GetSpecialValueList();
            }
        }
        #endregion

        #region button events
        public void AddSelectedValue()
        {
            SpecialValue.CopiedValue = SpecialValue.GetValueAgainstSpecialValue(SelectedSpecialValue);
        }
        #endregion

    }
}
