#region File Information/History
// <copyright file="ListControlGridViewModel.cs" project="WCFLoadUI" >
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
using Common;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;
using System.Collections.Generic;
using WCFLoad.Helper;
using Common.Infrastructure.Entities;

namespace WCFLoadUI.ViewModels
{
    public class ListControlGridViewModel : BaseViewModel
    {
        #region private fields
        private ObservableCollection<ControlView> _properties;
        private BindingList<IntWrappper> _arrayIndexes;
        private Type _fieldType;
        private string _fieldTypeName;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private ControlView _baseTypeProperties;
        private List<List<ArrayItem>> _listOfListItemGrid;
        #endregion

        #region constructor
        public ListControlGridViewModel(ObservableCollection<ControlView> properties,
            BindingList<IntWrappper> arrayIndexes,
            Type fieldType,
            ControlView baseTypeProperties
            )
        {
            Properties = properties;
            ArrayIndexes = arrayIndexes;
            FieldType = fieldType;
            _baseTypeProperties = baseTypeProperties;

            int x = 1;
            if (ArrayIndexes.Count == 1) x = 1;
            else if (ArrayIndexes.Count > 1)
            {
                for (var i = 0; i < ArrayIndexes.Count - 1; i++)
                {
                    x = x * ArrayIndexes[i].Int;
                }
            }
            int y = ArrayIndexes[ArrayIndexes.Count - 1].Int;

            _listOfListItemGrid = new List<List<ArrayItem>>();

            int totalElementsInList = arrayIndexes.Aggregate(1, (current, iw) => current * iw.Int);
            if (Properties.Count < totalElementsInList)
            {
                for (int i = Properties.Count; i < totalElementsInList; i++)
                {
                    Properties.Add(Controller.DeepCopy(_baseTypeProperties));
                }
            }

            var indexCount = 0;
            List<ArrayItem> l = new List<ArrayItem>();
            List<int> arrayIndexesInt = arrayIndexes.Select(o => o.Int).ToList();
            for (var j = 0; j < x * y; j++)
            {
                if (j % y == 0)
                {
                    l = new List<ArrayItem>();
                    _listOfListItemGrid.Add(l);
                }

                List<int> indexes = new List<int>();

                for (int ir = 0; ir < arrayIndexesInt.Count; ir++)
                {
                    TestHelper.GetArrayIndexesFromLinearIndex(arrayIndexesInt, ir, indexes, j);
                }

                l.Add(new ArrayItem()
                {
                    Index = Convert.ToString(indexCount),
                    DisplayIndex = string.Join(",", indexes.ToArray())
                });
                indexCount++;
            }

            NotifyOfPropertyChange(() => ListOfListItemGrid);
        }
        #endregion

        #region public properties
        public string ArrayIndexesStr
        {
            get
            {
                return string.Join(",", ArrayIndexes.Select(i => i.Int).ToArray());
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

        public BindingList<IntWrappper> ArrayIndexes
        {
            get
            {
                return _arrayIndexes;
            }
            set
            {
                _arrayIndexes = value;
                NotifyOfPropertyChange(() => ArrayIndexes);
            }
        }

        public List<List<ArrayItem>> ListOfListItemGrid
        {
            get { return _listOfListItemGrid; }
            set
            {
                _listOfListItemGrid = value;
                NotifyOfPropertyChange(() => ListOfListItemGrid);
            }
        }

        #endregion

        #region button events
        public void AddUpdateRecord(object context)
        {
            var itemIndex = Convert.ToInt32(context);
            Controller.ShowListViewItemViewDialog(Properties.ElementAt(itemIndex));
        }
        #endregion
    }
}
