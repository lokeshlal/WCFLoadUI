#region File Information/History
// <copyright file="AddNotesViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.Collections.ObjectModel;
using WCFLoad;
using WCFLoadUI.Base;

namespace WCFLoadUI.ViewModels
{
    public class AddNotesViewModel : BaseViewModel
    {
        #region private properties
        private const string WindowTitleDefault = "Add Nodes";
        private string _windowTitle = WindowTitleDefault;
        private string _selectedNode;
        private string _newNode;
        private ObservableCollection<string> _nodes;
        private int _noOfClientsPerNode;
        #endregion

        #region public properties
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        public string NewNode
        {
            get { return _newNode; }
            set
            {
                _newNode = value;
                NotifyOfPropertyChange(() => NewNode);
            }
        }

        public ObservableCollection<string> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                NotifyOfPropertyChange(() => Nodes);
            }
        }

        public string SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                NotifyOfPropertyChange(() => SelectedNode);
            }
        }

        public int NoOfClientsPerNode
        {
            get { return _noOfClientsPerNode; }
            set
            {
                if (value <= 0)
                {
                    return;
                }
                _noOfClientsPerNode = value;
                TestEngine.TestPackage.Nodes.NoOfClientsPerNode = _noOfClientsPerNode;
                NotifyOfPropertyChange(() => NoOfClientsPerNode);
            }
        }

        #endregion

        #region window events
        /// <summary>
        /// On View loaded event of caliburn
        /// </summary>
        /// <param name="view"></param>
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            Nodes = new ObservableCollection<string>(TestEngine.TestPackage.Nodes.NodeList);

            if (TestEngine.TestPackage.Nodes.NoOfClientsPerNode > 0)
            {
                NoOfClientsPerNode = TestEngine.TestPackage.Nodes.NoOfClientsPerNode;
            }
            else
            {
                NoOfClientsPerNode = TestEngine.TestPackage.Nodes.NoOfClientsPerNode = 1;
            }
        }
        #endregion

        #region button events
        /// <summary>
        /// Add a new node as agent
        /// </summary>
        public void AddNode()
        {
            if (!string.IsNullOrEmpty(NewNode))
            {
                if (!Nodes.Contains(NewNode))
                {
                    TestEngine.TestPackage.Nodes.NodeList.Add(NewNode);
                    Nodes = new ObservableCollection<string>(TestEngine.TestPackage.Nodes.NodeList);
                }
            }

            NewNode = string.Empty;
        }

        /// <summary>
        /// removes a node from nodelist
        /// </summary>
        public void RemoveNode()
        {
            if (!string.IsNullOrEmpty(SelectedNode))
            {
                TestEngine.TestPackage.Nodes.NodeList.Remove(SelectedNode);
                Nodes = new ObservableCollection<string>(TestEngine.TestPackage.Nodes.NodeList);
            }
        }
        #endregion
    }
}
