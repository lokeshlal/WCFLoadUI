#region File Information/History
// <copyright file="PrimitiveControlViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WCFLoad.Helper;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{

    [Serializable]
    public class PrimitiveControlViewModel : BaseViewModel, ISerializable
    {
        #region private fields
        private string _fieldName;
        private string _fieldValue;
        private Type _fieldType;
        private int _order;
        private string _fieldTypeName;

        private List<EnumBinding> _enumValues;
        private int _selectedEnumValue;
        private EnumBinding _selectedEnumItem;
        private int _selectedEnumIndex;
        #endregion

        #region contructor
        public PrimitiveControlViewModel()
        {
            PopulateEnumValue();
        }

        public PrimitiveControlViewModel(SerializationInfo info, StreamingContext context)
        {
            FieldName = (string)info.GetValue("FieldName", typeof(string));
            //FieldType = (Type)info.GetValue("FieldType", typeof(Type));
            FieldValue = (string)info.GetValue("FieldValue", typeof(string));
            Order = (int)info.GetValue("Order", typeof(int));
            FieldTypeName = (string)info.GetValue("FieldTypeName", typeof(string));
            AssemblyGuid = (string)info.GetValue("AssemblyGuid", typeof(string));

            FieldType = Controller.GetBaseTypeFromCurrentAssembly(FieldTypeName, AssemblyGuid);
            PopulateEnumValue();

        }
        #endregion

        #region public properties
        public List<string> SpecialValueList
        {
            get
            {
                return SpecialValue.GetSpecialValueList();
            }
        }


        public List<EnumBinding> EnumValues
        {
            get { return _enumValues; }
            set
            {
                _enumValues = value;
                NotifyOfPropertyChange(() => EnumValues);
            }
        }

        public int SelectedEnumIndex
        {
            get { return _selectedEnumIndex; }
            set
            {
                if (value == -1)
                    return;
                _selectedEnumIndex = value;
                NotifyOfPropertyChange(() => SelectedEnumIndex);
            }
        }
        public int SelectedEnumValue
        {
            get { return _selectedEnumValue; }
            set
            {
                _selectedEnumValue = value;
                _fieldValue = Convert.ToString(_selectedEnumValue);
                NotifyOfPropertyChange(() => SelectedEnumValue);


                SelectedEnumItem = EnumValues.Single(e => e.Id == SelectedEnumValue);
                SelectedEnumIndex = EnumValues.IndexOf(SelectedEnumItem);

                //NotifyOfPropertyChange(() => SelectedEnumItem);
            }
        }
        public EnumBinding SelectedEnumItem
        {
            get { return _selectedEnumItem; }
            set
            {
                if (value == null)
                    return;
                _selectedEnumItem = value;
                _fieldValue = Convert.ToString(_selectedEnumItem.Id);

                NotifyOfPropertyChange(() => SelectedEnumItem);
            }
        }

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
                PopulateEnumValue();
                NotifyOfPropertyChange(() => FieldType);
            }
        }


        public string FieldValue
        {
            get
            {
                if (string.IsNullOrEmpty(_fieldValue))
                    return string.Empty;
                return _fieldValue;
            }
            set
            {
                _fieldValue = value;

                if (FieldType != null && FieldType.Name.ToLower() == "datetime")
                {
                    if (!string.IsNullOrEmpty(_fieldValue))
                    {
                        DateTime dt;
                        if (DateTime.TryParse(_fieldValue, out dt))
                        {
                            //ok
                        }
                        else if (_fieldValue == "{DateTime.Now}")
                        {
                            //ok
                        }
                        else
                        {
                            _fieldValue = string.Empty;
                        }
                    }
                }

                if (FieldType != null && FieldType.IsEnum)
                {
                    if (!string.IsNullOrEmpty(FieldValue))
                    {
                        _selectedEnumValue = Convert.ToInt32(_fieldValue);
                        _selectedEnumItem = EnumValues.Single(e => e.Id == SelectedEnumValue);

                        NotifyOfPropertyChange(() => SelectedEnumValue);
                        NotifyOfPropertyChange(() => SelectedEnumItem);
                    }
                }
                NotifyOfPropertyChange(() => FieldValue);
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

        public string AssemblyGuid { get; set; }

        #endregion


        #region private methods
        /// <summary>
        /// method to populate enum values
        /// </summary>
        private void PopulateEnumValue()
        {
            if (FieldType != null && FieldType.IsEnum)
            {
                string[] names = Enum.GetNames(FieldType);
                Array values = Enum.GetValues(FieldType);
                List<EnumBinding> tmpEnumValues = names.Select((t, i) => new EnumBinding()
                {
                    Id = Convert.ToInt32(values.GetValue(i)),
                    Name = t
                }).ToList();
                EnumValues = tmpEnumValues;

                if (!string.IsNullOrEmpty(FieldValue))
                {
                    SelectedEnumValue = Convert.ToInt32(_fieldValue);
                    //SelectedEnumItem = EnumValues.Single(e => e.ID == SelectedEnumValue);
                }
            }
        }
        #endregion

        #region button events
        public void FieldNameControl()
        {
            if (!string.IsNullOrEmpty(SpecialValue.CopiedValue))
            {
                FieldValue = SpecialValue.CopiedValue;
            }
        }
        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FieldName", FieldName);
            info.AddValue("AssemblyGuid", AssemblyGuid);
            //info.AddValue("FieldType", FieldType);
            info.AddValue("FieldValue", FieldValue);
            info.AddValue("Order", Order);
            info.AddValue("FieldTypeName", FieldTypeName);
        }
    }
}
