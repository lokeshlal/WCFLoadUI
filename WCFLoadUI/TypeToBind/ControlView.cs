#region File Information/History
// <copyright file="ControlView.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Runtime.Serialization;
using WCFLoadUI.ViewModels;

namespace WCFLoadUI.TypeToBind
{
    /// <summary>
    /// Parent type for a object of type primitive, enums, complex, list or dictionary
    /// </summary>
    [Serializable]
    public class ControlView : ISerializable
    {
        #region private fields
        private PrimitiveControlViewModel _pControlView;
        private ComplexControlViewModel _cControlView;
        private ListControlViewModel _lControlView;
        private DictionartControlViewModel _dControlView;
        private bool _isPrimitive;
        private bool _isList;
        private bool _isDict;
        #endregion

        #region constructor
        public ControlView()
        {

        }

        public ControlView(SerializationInfo info, StreamingContext context)
        {
            PControlView = (PrimitiveControlViewModel)info.GetValue("PControlView", typeof(PrimitiveControlViewModel));
            CControlView = (ComplexControlViewModel)info.GetValue("CControlView", typeof(ComplexControlViewModel));
            LControlView = (ListControlViewModel)info.GetValue("LControlView", typeof(ListControlViewModel));
            DControlView = (DictionartControlViewModel)info.GetValue("DControlView", typeof(DictionartControlViewModel));
            IsPrimitive = (bool)info.GetValue("IsPrimitive", typeof(bool));
            IsList = (bool)info.GetValue("IsList", typeof(bool));
            IsDictionary = (bool)info.GetValue("IsDictionary", typeof(bool));
        }
        #endregion

        #region public properties
        public PrimitiveControlViewModel PControlView
        {
            get
            {
                return _pControlView;
            }
            set
            {
                _pControlView = value;
            }
        }

        public ComplexControlViewModel CControlView
        {
            get
            {
                return _cControlView;
            }
            set
            {
                _cControlView = value;
            }
        }

        public ListControlViewModel LControlView
        {
            get
            {
                return _lControlView;
            }
            set
            {
                _lControlView = value;
            }
        }

        public DictionartControlViewModel DControlView
        {
            get
            {
                return _dControlView;
            }
            set
            {
                _dControlView = value;
            }
        }

        public bool IsPrimitive
        {
            get
            {
                return _isPrimitive;
            }
            set
            {
                _isPrimitive = value;
            }
        }

        public bool IsList
        {
            get
            {
                return _isList;
            }
            set
            {
                _isList = value;
            }
        }

        public bool IsDictionary
        {
            get
            {
                return _isDict;
            }
            set
            {
                _isDict = value;
            }
        }

        public int Type
        {
            get
            {
                if (IsDictionary)
                {
                    return 3; //Dictionary
                }
                else if (IsList)
                {
                    return 2; //List
                }
                else if (IsPrimitive)
                {
                    return 0; //Primitive
                }
                else
                {
                    return 1;  //Complex
                }
            }
        }
        #endregion

        #region ISerializable method
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PControlView", PControlView);
            info.AddValue("CControlView", CControlView);
            info.AddValue("LControlView", LControlView);
            info.AddValue("DControlView", DControlView);
            info.AddValue("IsPrimitive", IsPrimitive);
            info.AddValue("IsList", IsList);
            info.AddValue("IsDictionary", IsDictionary);
        }
        #endregion

    }
}
