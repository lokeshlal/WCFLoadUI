#region File Information/History
// <copyright file="DictionartControlViewModel.cs" project="WCFLoadUI" >
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
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    [Serializable]
    public class DictionartControlViewModel : BaseViewModel, ISerializable
    {
        #region private fields
        private string _fieldName;
        private Type _fieldType;
        private string _fieldTypeName;
        private Type _fieldValueType;
        private string _fieldValueTypeName;
        private int _dictionaryItemsCount;
        private ObservableCollection<ControlView> _properties;
        private ObservableCollection<ControlView> _propertiesValue;
        private ControlView _baseTypeProperties;
        private ControlView _baseValueTypeProperties;
        private int _order;
        #endregion

        #region constructor
        public DictionartControlViewModel()
        {
            _properties = new ObservableCollection<ControlView>();
            _propertiesValue = new ObservableCollection<ControlView>();
        }

        public DictionartControlViewModel(SerializationInfo info, StreamingContext context)
        {
            FieldName = (string)info.GetValue("FieldName", typeof(string));
            Properties = (ObservableCollection<ControlView>)info.GetValue("Properties", typeof(ObservableCollection<ControlView>));
            PropertiesValue = (ObservableCollection<ControlView>)info.GetValue("PropertiesValue", typeof(ObservableCollection<ControlView>));
            BaseTypeProperties = (ControlView)info.GetValue("BaseTypeProperties", typeof(ControlView));
            BaseValueTypeProperties = (ControlView)info.GetValue("BaseValueTypeProperties", typeof(ControlView));
            FieldTypeName = (string)info.GetValue("FieldTypeName", typeof(string));
            FieldValueTypeName = (string)info.GetValue("FieldValueTypeName", typeof(string));
            DictionaryItemsCount = (int)info.GetValue("DictionaryItemsCount", typeof(int));
            Order = (int)info.GetValue("Order", typeof(int));
            AssemblyGuid = (string)info.GetValue("AssemblyGuid", typeof(string));

            FieldType = Controller.GetBaseTypeFromCurrentAssembly(FieldTypeName, AssemblyGuid);
            FieldValueType = Controller.GetBaseTypeFromCurrentAssembly(FieldValueTypeName, AssemblyGuid);
        }

        public string AssemblyGuid { get; set; }

        #endregion

        #region public properties
        public ControlView BaseTypeProperties
        {
            get
            {
                return _baseTypeProperties;
            }
            set
            {
                _baseTypeProperties = value;
                NotifyOfPropertyChange(() => BaseTypeProperties);
            }
        }

        public ControlView BaseValueTypeProperties
        {
            get
            {
                return _baseValueTypeProperties;
            }
            set
            {
                _baseValueTypeProperties = value;
                NotifyOfPropertyChange(() => BaseValueTypeProperties);
            }
        }

        public Type FieldType
        {
            get
            {
                return _fieldType;
            }
            set
            {
                _fieldType = value;
                if (string.IsNullOrEmpty(_fieldTypeName))
                {
                    FieldTypeName = _fieldType.FullName;
                }
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
                if (string.IsNullOrEmpty(_fieldValueTypeName))
                {
                    FieldValueTypeName = _fieldValueType.FullName;
                }
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

        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                _fieldName = value;
                NotifyOfPropertyChange(() => FieldName);
            }
        }

        public int DictionaryItemsCount
        {
            get
            {
                return _dictionaryItemsCount;
            }
            set
            {
                _dictionaryItemsCount = value;
                NotifyOfPropertyChange(() => CanDisplayShowArrayGrid);
                NotifyOfPropertyChange(() => DictionaryItemsCount);
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

        public bool CanDisplayShowArrayGrid
        {
            get
            {
                if (_dictionaryItemsCount < 0)
                    return false;
                return true;
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

        #region button events
        public void ShowArrayGrid()
        {
            //display popup with array details
            Controller.ShowDictionaryControlGridViewDialog(Properties,
                PropertiesValue,
                DictionaryItemsCount,
                FieldType,
                FieldValueType,
                BaseTypeProperties,
                BaseValueTypeProperties);
        }
        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FieldName", FieldName);
            info.AddValue("AssemblyGuid", AssemblyGuid);
            info.AddValue("Properties", Properties);
            info.AddValue("PropertiesValue", PropertiesValue);
            info.AddValue("BaseTypeProperties", BaseTypeProperties);
            info.AddValue("BaseValueTypeProperties", BaseValueTypeProperties);
            info.AddValue("DictionaryItemsCount", DictionaryItemsCount);
            info.AddValue("Order", Order);
            info.AddValue("FieldTypeName", FieldTypeName);
            info.AddValue("FieldValueTypeName", FieldValueTypeName);
        }
    }
}
