#region File Information/History
// <copyright file="DictionaryControlGridViewModel.cs" project="WCFLoadUI" >
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
using System.Linq;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    public class DictionaryControlGridViewModel : BaseViewModel
    {
        #region private fields
        private ObservableCollection<ControlView> _properties;
        private ObservableCollection<ControlView> _propertiesValue;
        private int _itemsCount;
        private Type _fieldType;
        private Type _fieldValueType;
        private string _fieldTypeName;
        private string _fieldValueTypeName;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private ControlView _baseTypeProperties;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private ControlView _baseValueTypeProperties;
        #endregion

        #region constructor
        public DictionaryControlGridViewModel(ObservableCollection<ControlView> properties,
            ObservableCollection<ControlView> propertiesValue,
            int itemsCount,
            Type fieldType,
            Type fieldValueType,
            ControlView baseTypeProperties,
            ControlView baseValueTypeProperties
            )
        {
            Properties = properties;
            PropertiesValue = propertiesValue;
            ItemsCount = itemsCount;
            FieldType = fieldType;
            FieldValueType = fieldValueType;
            _baseTypeProperties = baseTypeProperties;
            _baseValueTypeProperties = baseValueTypeProperties;

            if (Properties.Count < _itemsCount)
            {
                for (int i = Properties.Count; i < _itemsCount; i++)
                {
                    Properties.Add(Controller.DeepCopy(_baseTypeProperties));
                }
            }
            if (PropertiesValue.Count < _itemsCount)
            {
                for (int i = PropertiesValue.Count; i < _itemsCount; i++)
                {
                    PropertiesValue.Add(Controller.DeepCopy(_baseValueTypeProperties));
                }
            }
        }
        #endregion

        #region public properties
        public Type FieldType
        {
            get
            {
                return _fieldType;
            }
            set
            {
                _fieldType = value;
                NotifyOfPropertyChange(() => FieldType);
            }
        }

        public Type FieldValueType
        {
            get
            {
                return _fieldValueType;
            }
            set
            {
                _fieldValueType = value;
                NotifyOfPropertyChange(() => FieldValueType);
            }
        }

        public string FieldTypeName
        {
            get
            {
                return _fieldTypeName;
            }
            set
            {
                _fieldTypeName = value;
                NotifyOfPropertyChange(() => FieldTypeName);
            }
        }
        public string FieldValueTypeName
        {
            get
            {
                return _fieldValueTypeName;
            }
            set
            {
                _fieldValueTypeName = value;
                NotifyOfPropertyChange(() => FieldValueTypeName);
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

        public ObservableCollection<ControlView> PropertiesValue
        {
            get
            {
                return _propertiesValue;
            }
            set
            {
                _propertiesValue = value;
                NotifyOfPropertyChange(() => PropertiesValue);
            }
        }

        public int ItemsCount
        {
            get
            {
                return _itemsCount;
            }
            set
            {
                _itemsCount= value;
                NotifyOfPropertyChange(() => ItemsCount);
            }
        }
        #endregion

        #region button events
        public void AddUpdateRecord(object context)
        {
            var itemIndex = Convert.ToInt32(context);
            Controller.ShowListViewItemViewDialog(Properties.ElementAt(itemIndex));
        }

        public void AddUpdateRecordValue(object context)
        {
            var itemIndex = Convert.ToInt32(context);
            Controller.ShowListViewItemViewDialog(PropertiesValue.ElementAt(itemIndex));
        }
        #endregion
    }
}
