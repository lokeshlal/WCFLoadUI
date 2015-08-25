#region File Information/History
// <copyright file="TestClasses.cs" project="Common" >
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
using System.Net;
using System.ServiceModel.Description;

//Classes to hold performance test

namespace Common
{
    [Serializable]
    public enum MethodStatus
    {
        PreThread = 0,
        Started = 1,
        Pass = 2,
        Fail = 3,
        Error = 4
    }

    [Serializable]
    public class MethodLogs
    {
        public int Total { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public double AvgTimeTaken { get; set; }
        public long Maxtime { get; set; }
        public long MinTime { get; set; }
        public List<int> TimeTaken { get; set; }
        public int PreThread { get; set; }
    }

    [Serializable]
    public class Package
    {
        public Package()
        {
            Suites = new List<TestSuite>();
            Nodes = new Nodes();
            RestMethods = new List<RestMethod>();
        }
        public List<TestSuite> Suites { get; set; }
        public List<RestMethod> RestMethods { get; set; }
        public string ResultFileName { get; set; }

        public int IntervalBetweenScenarios;
        public List<Scenario> Scenarios = new List<Scenario>();
        public Nodes Nodes { get; set; }
        public int Duration { get; set; }
        public int Clients { get; set; }
        public int DelayRangeStart { get; set; }
        public int DelayRangeEnd { get; set; }

    }

    [Serializable]
    public enum RequestType
    {
        Get,
        Post,
        Delete,
        Put,
        Patch,
        Head,
        Options
    }

    [Serializable]
    public class KeyValue
    {
        private string _key;
        private string _value;

        public KeyValue()
        {
            _key = string.Empty;
            _value = string.Empty;
        }


        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    [Serializable]
    public class RestMethodResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Response { get; set; }
    }

    [Serializable]
    public class RestMethod
    {
        #region private fields
        private string _guid;
        private string _url;
        private string _contentType;
        private List<KeyValue> _headers;
        private string _headerText;
        private List<KeyValue> _payloadValues;
        private string _payloadText;
        private RequestType _type;
        private int _selectedHeaderTab;
        private int _selectedPayloadTab;
        private bool _isAddedToFunctional;
        private string _methodOutput;
        #endregion

        #region constructor
        public RestMethod()
        {
            var g = System.Guid.NewGuid();
            //Set default values
            _headers = new List<KeyValue>();
            _payloadValues = new List<KeyValue>();
            _headerText = string.Empty;
            _payloadText = string.Empty;
            _url = string.Empty;
            _guid = g.ToString();
            _type = RequestType.Get;
            _methodOutput = string.Empty;
            _contentType = @"application/json";
        }
        #endregion

        #region public properties

        #region read-only properties

        public string DisplayName
        {
            get { return string.Format("{0} - {1}", Url, Guid); }
        }
        public string Protocol
        {
            get
            {
                if (!string.IsNullOrEmpty(_url))
                {
                    Uri uri = new Uri(_url);
                    return uri.Scheme;
                }
                return string.Empty;
            }
        }

        public int Port
        {
            get
            {
                if (!string.IsNullOrEmpty(_url))
                {
                    Uri uri = new Uri(_url);
                    return uri.Port;
                }
                return 80;
            }
        }

        public string AbsolutePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_url))
                {
                    Uri uri = new Uri(_url);
                    return uri.AbsolutePath;
                }
                return string.Empty;
            }
        }

        public string Host
        {
            get
            {
                if (!string.IsNullOrEmpty(_url))
                {
                    Uri uri = new Uri(_url);
                    return uri.Host;
                }
                return string.Empty;
            }
        }
        #endregion

        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public List<KeyValue> Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                //_headerText = _headerText.Replace(@"\r\n", " ");
            }
        }

        public List<KeyValue> PayloadValues
        {
            get { return _payloadValues; }
            set
            {
                _payloadValues = value;
            }
        }

        public string PayloadText
        {
            get { return _payloadText; }
            set
            {
                _payloadText = value;
                //_payloadText = _payloadText.Replace(@"\r\n", " ");
            }
        }

        public RequestType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int SelectedHeaderTab
        {
            get { return _selectedHeaderTab; }
            set { _selectedHeaderTab = value; }
        }

        public int SelectedPayloadTab
        {
            get { return _selectedPayloadTab; }
            set { _selectedPayloadTab = value; }
        }

        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        public bool IsAddedToFunctional
        {
            get { return _isAddedToFunctional; }
            set { _isAddedToFunctional = value; }
        }

        public string MethodOutput
        {
            get { return _methodOutput; }
            set { _methodOutput = value; }
        }

        #endregion
    }



    [Serializable]
    public class TestSuite
    {
        public Dictionary<string, string> EndPoints = new Dictionary<string, string>();
        public Dictionary<string, string> EndPointType = new Dictionary<string, string>();
        public Dictionary<string, IEnumerable<ServiceEndpoint>> EndpointsForContracts = new Dictionary<string, IEnumerable<ServiceEndpoint>>();

        public TestSuite()
        {
            FunctionalTests = new List<Test>();
            Tests = new List<Test>();
        }
        public string Guid { get; set; }
        public List<Test> Tests { get; set; }
        public List<Test> FunctionalTests { get; set; }
        public string Wsdl { get; set; }
        public string BaseUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string Configuration { get; set; }
        public string BindingToTest { get; set; }
    }

    [Serializable]
    public class Nodes
    {
        public Nodes()
        {
            NodeList = new List<string>();
            NoOfClientsPerNode = 1;
        }
        public int NoOfClientsPerNode { get; set; }
        public List<string> NodeList { get; set; }
    }

    [Serializable]
    public class Scenario
    {
        public List<ScenarioOrder> ScenarioOrder = new List<ScenarioOrder>();
    }

    [Serializable]
    public class ScenarioOrder
    {
        public string MethodName { get; set; }
        public string MethodGuid { get; set; }
        public string AssemblyGuid { get; set; }
        public bool IsRest { get; set; }
        public int Order { get; set; }
    }

    [Serializable]
    public class Test
    {
        public Service Service { get; set; }
    }

    [Serializable]
    public class Service
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public bool IsAsync { get; set; }
        public Parameter Parameters { get; set; }
        public Values Values { get; set; }
    }

    [Serializable]
    public class Parameter
    {
        public List<Param> Param { get; set; }
    }

    [Serializable]
    public class Param
    {
        public string Type { get; set; }
        public int Order { get; set; }
        public bool IsComplex { get; set; }
        public List<Property> Properties { get; set; }
    }

    [Serializable]
    public class Property
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    [Serializable]
    public class Values
    {
        public List<Value> ValueList { get; set; }
    }

    [Serializable]
    public class Value
    {
        public List<ParamValue> Param { get; set; }
        public string MethodOutput { get; set; }
        public string Guid { get; set; }
    }

    [Serializable]
    public class ParamValue
    {
        public ParamValue()
        {
            GuidValues = new List<ParamValue>();
            ArrayElements = new List<ParamValue>();
            ArrayIndexes = new List<IntWrappper>();

            DictKeyElements = new List<ParamValue>();
            DictValueElements = new List<ParamValue>();
        }

        public int Order { get; set; }
        public Dictionary<string, string> Param { get; set; }
        public string Guid { get; set; }
        public string Type { get; set; }
        public List<ParamValue> GuidValues { get; set; }
        //---------------------------------
        //FOR ARRAYS
        //---------------------------------
        public bool IsArray { get; set; }
        public List<IntWrappper> ArrayIndexes { get; set; }
        public List<ParamValue> ArrayElements { get; set; }
        //---------------------------------

        //---------------------------------
        //FOR Dictionary
        //---------------------------------
        public bool IsDictionary { get; set; }
        public int DictionaryLength { get; set; }
        public List<ParamValue> DictKeyElements { get; set; }
        public List<ParamValue> DictValueElements { get; set; }
    }

    [Serializable]
    public class IntWrappper
    {
        private int _int;

        public int Int
        {
            get { return _int; }
            set { _int = value; }
        }
    }

    [Serializable]
    public enum ParamType
    {
        ComplexObject,
        Array
    }
}