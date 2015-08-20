#region File Information/History
// <copyright file="ComplexControlViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    [Serializable]
    public class ComplexControlViewModel : BaseViewModel, ISerializable
    {
        #region private fields
        private ObservableCollection<PrimitiveControlViewModel> _propertiesNew; //properties;
        private ObservableCollection<ControlView> _properties; //properties_new
        private int _order;
        private string _fieldName;
        #endregion


        #region constructor
        public ComplexControlViewModel()
        {

        }

        public ComplexControlViewModel(SerializationInfo info, StreamingContext context)
        {
            FieldName = (string)info.GetValue("FieldName", typeof(string));
            Properties = (ObservableCollection<ControlView>)info.GetValue("Properties", typeof(ObservableCollection<ControlView>));
            PropertiesNew = (ObservableCollection<PrimitiveControlViewModel>)info.GetValue("Properties_New", typeof(ObservableCollection<PrimitiveControlViewModel>));
            Order = (int)info.GetValue("Order", typeof(int));
        }
        #endregion


        #region public properties
        public string FieldName
        {
            get
            {
                if (string.IsNullOrEmpty(_fieldName))
                    return string.Empty;
                return _fieldName;
            }
            set
            {
                _fieldName = value;
                NotifyOfPropertyChange(() => FieldName);
            }
        }
        public ObservableCollection<ControlView> Properties
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

        public ObservableCollection<PrimitiveControlViewModel> PropertiesNew
        {
            get
            {
                return _propertiesNew;
            }
            set
            {
                _propertiesNew = value;
                NotifyOfPropertyChange(() => PropertiesNew);
            }
        }

        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
            }
        }
        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FieldName", FieldName);
            info.AddValue("Properties", Properties);
            info.AddValue("Properties_New", PropertiesNew);
            info.AddValue("Order", Order);
        }
    }
}
