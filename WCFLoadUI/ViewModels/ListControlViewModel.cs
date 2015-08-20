#region File Information/History
// <copyright file="ListControlViewModel.cs" project="WCFLoadUI" >
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Common;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    [Serializable]
    public class ListControlViewModel : BaseViewModel, ISerializable
    {
        #region private fields
        private string _fieldName;
        private Type _fieldType;
        private string _fieldTypeName;
        private BindingList<IntWrappper> _arrayIndexes;
        private ObservableCollection<ControlView> _properties;
        private ControlView _baseTypeProperties;
        private int _order;
        #endregion

        #region consrtuctors
        public ListControlViewModel()
        {
            _properties = new ObservableCollection<ControlView>();
            ArrayIndexes = new BindingList<IntWrappper>();
        }

        public ListControlViewModel(SerializationInfo info, StreamingContext context)
        {
            FieldName = (string)info.GetValue("FieldName", typeof(string));
            Properties = (ObservableCollection<ControlView>)info.GetValue("Properties", typeof(ObservableCollection<ControlView>));
            BaseTypeProperties = (ControlView)info.GetValue("BaseTypeProperties", typeof(ControlView));
            //FieldType = (Type)info.GetValue("FieldType", typeof(Type));
            FieldTypeName = (string)info.GetValue("FieldTypeName", typeof(string));
            ArrayIndexes = (BindingList<IntWrappper>)info.GetValue("ArrayIndexes", typeof(BindingList<IntWrappper>));
            Order = (int)info.GetValue("Order", typeof(int));
            AssemblyGuid = (string)info.GetValue("AssemblyGuid", typeof(string));


            FieldType = Controller.GetBaseTypeFromCurrentAssembly(FieldTypeName, AssemblyGuid);
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

        public BindingList<IntWrappper> ArrayIndexes
        {
            get
            {
                return _arrayIndexes;
            }
            set
            {
                _arrayIndexes = value;
                NotifyOfPropertyChange(() => CanDisplayShowArrayGrid);
                NotifyOfPropertyChange(() => ArrayIndexes);
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

        public bool CanDisplayShowArrayGrid
        {
            get
            {
                if (ArrayIndexes.Any(o => o.Int < 0))
                    return false;
                return true;
            }
        }

        public void ShowArrayGrid()
        {
            //display popup with array details
            Controller.ShowListControlGridViewDialog(Properties, ArrayIndexes, FieldType, _baseTypeProperties);
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

        #region serialize method
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FieldName", FieldName);
            info.AddValue("AssemblyGuid", AssemblyGuid);
            info.AddValue("Properties", Properties);
            info.AddValue("BaseTypeProperties", BaseTypeProperties);
            //info.AddValue("FieldType", FieldType);
            info.AddValue("ArrayIndexes", ArrayIndexes);
            info.AddValue("Order", Order);
            info.AddValue("FieldTypeName", FieldTypeName);
        }
        #endregion
    }
}
