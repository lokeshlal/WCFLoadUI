#region File Information/History
// <copyright file="Test.cs" project="WCFLoad" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Common;
using FBServiceClient;
using WCFLoad.Helper;
using Binding = System.ServiceModel.Channels.Binding;
using Service = Common.Service;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;
using System.Collections.ObjectModel;

namespace WCFLoad
{
    /// <summary>
    /// Test class to perform the main task of loading proxy, loading test xml and executing the performance test
    /// </summary>
    public static class Test
    {
        #region private properties
        private static string _testConfigurationFile;
        private static Dictionary<string, Assembly> _assembly = new Dictionary<string, Assembly>();
        private static Dictionary<string, Type> _serviceInterface = new Dictionary<string, Type>();
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once CollectionNeverQueried.Local
        private static Dictionary<string, string> _endPointUrls = new Dictionary<string, string>();
        private const string CacheFile = "cache.txt";
        private static CancellationTokenSource _performanceRunTokenSource;
        private static CancellationToken _performanceRunToken;
        private static int _eventCount = -1;
        #endregion

        #region public properties
        public static int TotalResponsesRecieved;
        public static int TotalPass;
        public static int TotalFail;
        public static Package TestPackage { get; set; }
        //public static TestSuite Suite { get; set; }
        public static int EventCount { get { return _eventCount; } set { _eventCount = value; } }
        public static ConcurrentDictionary<string, MethodLogs> MethodLevelResults = new ConcurrentDictionary<string, MethodLogs>();
        public static ConcurrentDictionary<string, MethodLogs> MethodLevelReqStatus = new ConcurrentDictionary<string, MethodLogs>();

        public static event EventHandler RunResultUpdated;

        public static CancellationTokenSource PerformanceRunTokenSource
        {
            get { return _performanceRunTokenSource; }
            set
            {
                _performanceRunTokenSource = value;
                if (_performanceRunTokenSource != null)
                    PerformanceRunToken = _performanceRunTokenSource.Token;
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public static CancellationToken PerformanceRunToken
        {
            get { return _performanceRunToken; }
            set { _performanceRunToken = value; }
        }
        #endregion

        #region Clean the Test Class for new run
        public static void Clear()
        {
            _assembly = new Dictionary<string, Assembly>();
            _serviceInterface = new Dictionary<string, Type>();
            _endPointUrls.Clear();
            TotalResponsesRecieved = 0;
            TotalPass = 0;
            TotalFail = 0;
            EventCount = -1;
            TestPackage = new Package();
            MethodLevelResults.Clear();
            ClearResultHandler();
        }
        public static void ClearCache()
        {
            TotalResponsesRecieved = 0;
            TotalPass = 0;
            TotalFail = 0;
            EventCount = -1;
            if (File.Exists(Environment.CurrentDirectory + "\\" + CacheFile))
            {
                File.Delete(Environment.CurrentDirectory + "\\" + CacheFile);
            }
            File.Create(Environment.CurrentDirectory + "\\" + CacheFile);
        }
        #endregion

        #region private methods

        /// <summary>
        /// Insert a new method in local DB via FbService
        /// </summary>
        /// <param name="methodName">Method Name</param>
        /// <param name="token">unique token for method call</param>
        private static void InsertNewMethodLog(string methodName, string token)
        {
            ServiceClient.InsertNewMethodLog(methodName, token);
        }

        /// <summary>
        /// Update method status in local DB via FbService
        /// </summary>
        /// <param name="token">unique token for method call</param>
        /// <param name="methodName">Method Name</param>
        /// <param name="status">Status</param>
        /// <param name="timeTaken">Time taken</param>
        /// <param name="error">Error if any</param>
        private static void UpdateMethodLog(string token, string methodName, MethodStatus status, long? timeTaken = null, string error = null)
        {
            if (EventCount > 100 || EventCount == -1)
            {
                EventCount = 0;
                if (RunResultUpdated != null)
                {
                    RunResultUpdated(null, null);
                }
            }
            EventCount++;
            ServiceClient.UpdateMethodLog(token, methodName, status, timeTaken, error);

        }



        /// <summary>
        /// To get response from the url
        /// </summary>
        /// <param name="url">WSDL URL</param>
        /// <returns></returns>
        private static StreamReader GetHttpWebResponse(string url)
        {
            if (String.IsNullOrEmpty(url))
                throw new Exception("url Cannot be empty");

            HttpWebRequest lHttp = (HttpWebRequest)WebRequest.Create(url);

            lHttp.Timeout = 30000; // Timeout after 30 seconds
            lHttp.UseDefaultCredentials = true;

            HttpWebResponse lResponse = (HttpWebResponse)lHttp.GetResponse();

            Encoding enc = Encoding.GetEncoding(1252); // Windows default Code Page
            // ReSharper disable once AssignNullToNotNullAttribute
            return new StreamReader(lResponse.GetResponseStream(), enc);
        }

        /// <summary>
        /// Creates a new instance of proxy loaded in memory
        /// </summary>
        /// <returns>Service Proxy instance</returns>
        private static object GetNewServiceProxyInstance(string guid)
        {
            Type clientProxyType = _assembly[guid].GetTypes().First(
                             t => t.IsClass &&
                                 t.GetInterface(_serviceInterface[guid].Name) != null &&
                                 t.GetInterface(typeof(ICommunicationObject).Name) != null);

            Binding binding = null;
            TestSuite suite = TestPackage.Suites.Find(s => s.Guid == guid);
            string endPointUrl = string.Empty;
            string endPointType = string.Empty;
            if (string.IsNullOrEmpty(suite.BindingToTest))
            {

                if (suite.EndpointsForContracts.Count > 0)
                {
                    ServiceEndpoint serviceEndpoint = suite.EndpointsForContracts.ElementAt(0).Value.FirstOrDefault();
                    if (serviceEndpoint != null)
                    {
                        binding = serviceEndpoint.Binding;
                        endPointUrl = serviceEndpoint.Address.Uri.ToString();
                    }
                }
                if (string.IsNullOrEmpty(endPointUrl))
                {
                    if (suite.EndPoints.Count > 0)
                    {
                        endPointUrl = suite.EndPoints.FirstOrDefault().Value;
                        endPointType = suite.EndPointType.FirstOrDefault().Value;
                    }
                    else
                    {
                        //Fallback to basic url
                        endPointUrl = suite.ServiceUrl;
                    }
                }
            }
            else
            {
                if (suite.EndpointsForContracts.Count > 0)
                {
                    if (
                        suite.EndpointsForContracts.ElementAt(0)
                            .Value
                            .Any(se => se.Binding.Name == suite.BindingToTest))
                    {
                        var serviceEndpoint = suite.EndpointsForContracts.ElementAt(0).Value.First(se => se.Binding.Name == suite.BindingToTest);
                        binding = serviceEndpoint.Binding;
                        endPointUrl = serviceEndpoint.Address.Uri.ToString();

                    }
                }
                if (string.IsNullOrEmpty(endPointUrl))
                {
                    endPointUrl = suite.EndPoints.Count > 0 && suite.EndPoints.ContainsKey(suite.BindingToTest)
                        ? suite.EndPoints[suite.BindingToTest]
                        : suite.ServiceUrl;
                    endPointType = suite.EndPointType.Count > 0 && suite.EndPointType.ContainsKey(suite.BindingToTest)
                        ? suite.EndPointType[suite.BindingToTest]
                        : string.Empty;
                }
            }

            //fallback in case end points are not handled via contract bindings
            if (binding == null)
            {
                switch (endPointType)
                {
                    case "BasicHttpBinding":
                        binding = new BasicHttpBinding();
                        ((BasicHttpBinding) binding).MaxBufferSize = Int32.MaxValue;
                        ((BasicHttpBinding) binding).MaxReceivedMessageSize = Int32.MaxValue;
                        ((BasicHttpBinding) binding).MaxBufferPoolSize = Int32.MaxValue;
                        ((BasicHttpBinding) binding).ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
                        break;
                    case "WSHttpBinding":
                        binding = new WSHttpBinding();
                        ((WSHttpBinding) binding).MaxReceivedMessageSize = Int32.MaxValue;
                        ((WSHttpBinding) binding).MaxBufferPoolSize = Int32.MaxValue;
                        ((WSHttpBinding) binding).ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
                        break;
                    case "NetTcpBinding":
                        binding = new NetTcpBinding();
                        ((NetTcpBinding) binding).MaxBufferSize = Int32.MaxValue;
                        ((NetTcpBinding) binding).MaxReceivedMessageSize = Int32.MaxValue;
                        ((NetTcpBinding) binding).MaxBufferPoolSize = Int32.MaxValue;
                        ((NetTcpBinding) binding).ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
                        break;
                    case "":
                        binding = new WebHttpBinding();
                        ((WebHttpBinding) binding).MaxBufferSize = Int32.MaxValue;
                        ((WebHttpBinding) binding).MaxReceivedMessageSize = Int32.MaxValue;
                        ((WebHttpBinding) binding).MaxBufferPoolSize = Int32.MaxValue;
                        ((WebHttpBinding) binding).ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
                        break;
                    default:
                        binding = new BasicHttpBinding();
                        ((BasicHttpBinding) binding).MaxBufferSize = Int32.MaxValue;
                        ((BasicHttpBinding) binding).MaxReceivedMessageSize = Int32.MaxValue;
                        ((BasicHttpBinding) binding).MaxBufferPoolSize = Int32.MaxValue;
                        ((BasicHttpBinding) binding).ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
                        break;
                }
            }

            return _assembly[guid].CreateInstance(
                clientProxyType.Name,
                false,
                BindingFlags.CreateInstance,
                null,
                new object[] { binding, new EndpointAddress(endPointUrl) },
                //new object[] { new EndpointAddress(endPointUrl) },
                CultureInfo.CurrentCulture, null);
        }
        #endregion

        #region public methods
        public static void CallRunResultUpdatedEvent()
        {
            if (RunResultUpdated != null)
            {
                RunResultUpdated(null, null);
            }
        }

        public static void ClearResultHandler()
        {
            RunResultUpdated = null;
        }

        public static void GenerateProxyAssembly(string url, string guid, bool fromUi = false)
        {
            TestSuite suite = TestPackage.Suites.Find(s => s.Guid == guid);

            if (suite == null && fromUi)
            {
                suite = new TestSuite()
                {
                    ServiceUrl = url,
                    BaseUrl = url,
                    Wsdl = url + "?wsdl",
                    Guid = guid
                };
                TestPackage.Suites.Add(suite);
                url = suite.Wsdl;
            }


            StreamReader lResponseStream = GetHttpWebResponse(url);

            XmlTextReader xmlreader = new XmlTextReader(lResponseStream);

            //read the downloaded WSDL file
            ServiceDescription desc = ServiceDescription.Read(xmlreader);

            MetadataSection section = MetadataSection.CreateFromServiceDescription(desc);
            MetadataSet metaDocs = new MetadataSet(new[] { section });
            WsdlImporter wsdlimporter = new WsdlImporter(metaDocs);

            //Add any imported files
            foreach (XmlSchema wsdlSchema in desc.Types.Schemas)
            {
                foreach (XmlSchemaObject externalSchema in wsdlSchema.Includes)
                {
                    var import = externalSchema as XmlSchemaImport;
                    if (import != null)
                    {
                        if (suite != null)
                        {
                            Uri baseUri = new Uri(suite.BaseUrl);
                            if (string.IsNullOrEmpty(import.SchemaLocation)) continue;
                            Uri schemaUri = new Uri(baseUri, import.SchemaLocation);
                            StreamReader sr = GetHttpWebResponse(schemaUri.ToString());
                            XmlSchema schema = XmlSchema.Read(sr, null);
                            wsdlimporter.XmlSchemas.Add(schema);
                        }
                    }
                }
            }

            //Additional check in case some services do not generate end points using generator
            for (int w = 0; w < wsdlimporter.WsdlDocuments.Count; w++)
            {
                for (int se = 0; se < wsdlimporter.WsdlDocuments[w].Services.Count; se++)
                {
                    for (int po = 0; po < wsdlimporter.WsdlDocuments[w].Services[se].Ports.Count; po++)
                    {
                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (int ext = 0; ext < wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions.Count; ext++)
                        {

                            switch (wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext].GetType().Name)
                            {
                                //BasicHttpBinding
                                case "SoapAddressBinding":
                                    _endPointUrls.Add(((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name, ((SoapAddressBinding)(wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext])).Location);
                                    if (suite != null &&
                                        !suite.EndPoints.ContainsKey(
                                            ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name))
                                    {
                                        suite.EndPoints.Add(
                                            ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name,
                                            ((SoapAddressBinding)
                                                (wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext]))
                                                .Location);
                                        suite.EndPointType.Add(((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name,
                                            "BasicHttpBinding");
                                    }
                                    break;
                                //WSHttpBinding
                                case "Soap12AddressBinding":
                                    _endPointUrls.Add(((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name, ((SoapAddressBinding)(wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext])).Location);
                                    if (suite != null &&
                                        !suite.EndPoints.ContainsKey(
                                            ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name))
                                    {
                                        if (((SoapAddressBinding)
                                            (wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext]))
                                            .Location.ToLower().StartsWith("net.tcp"))
                                        {
                                            suite.EndPoints.Add(
                                               ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name,
                                               ((SoapAddressBinding)
                                                   (wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext
                                                       ]))
                                                   .Location);
                                            suite.EndPointType.Add(
                                                ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name,
                                                "NetTcpBinding");
                                        }
                                        else
                                        {
                                            suite.EndPoints.Add(
                                                ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name,
                                                ((SoapAddressBinding)
                                                    (wsdlimporter.WsdlDocuments[w].Services[se].Ports[po].Extensions[ext
                                                        ]))
                                                    .Location);
                                            suite.EndPointType.Add(
                                                ((wsdlimporter.WsdlDocuments[w].Services[se].Ports[po])).Binding.Name,
                                                "WSHttpBinding");
                                        }
                                    }
                                    break;
                                case "XmlElement":
                                    break;
                            }
                        }
                    }
                }
            }




            foreach (Import import in wsdlimporter.WsdlDocuments[0].Imports)
            {
                GenerateProxyAssembly(import.Location, guid);
                return;
            }

            XsdDataContractImporter xsd = new XsdDataContractImporter
            {
                Options = new ImportOptions
                {
                    ImportXmlType = true,
                    GenerateSerializable = true
                }
            };
            xsd.Options.ReferencedTypes.Add(typeof(KeyValuePair<string, string>));
            xsd.Options.ReferencedTypes.Add(typeof(List<KeyValuePair<string, string>>));

            wsdlimporter.State.Add(typeof(XsdDataContractImporter), xsd);

            Collection<ContractDescription> contracts = wsdlimporter.ImportAllContracts();
            ServiceEndpointCollection allEndpoints = wsdlimporter.ImportAllEndpoints();
            // Generate type information for each contract.
            ServiceContractGenerator serviceContractGenerator = new ServiceContractGenerator();

            foreach (var contract in contracts)
            {
                serviceContractGenerator.GenerateServiceContractType(contract);
                // Keep a list of each contract's endpoints.
                if (suite != null)
                {
                    suite.EndpointsForContracts[contract.Name] =
                        allEndpoints.Where(ep => ep.Contract.Name == contract.Name).ToList();
                }
            }

            if (serviceContractGenerator.Errors.Count != 0)
                throw new Exception("There were errors during code compilation.");


            //Initialize the CODE DOM tree in which we will import the ServiceDescriptionImporter
            CodeNamespace nm = new CodeNamespace();
            CodeCompileUnit unit = new CodeCompileUnit();
            unit.Namespaces.Add(nm);

            CodeDomProvider compiler = CodeDomProvider.CreateProvider("C#");
            // include the assembly references needed to compile
            var references = new[] { "System.Web.Services.dll", "System.Xml.dll", "System.ServiceModel.dll", "System.configuration.dll", "System.Runtime.Serialization.dll" };
            var parameters = new CompilerParameters(references) { GenerateInMemory = true };
            var results = compiler.CompileAssemblyFromDom(parameters, serviceContractGenerator.TargetCompileUnit);

            if (results.Errors.Cast<CompilerError>().Any())
            {
                throw new Exception("Compilation Error Creating Assembly");
            }

            // all done....
            if (_assembly.ContainsKey(guid))
                _assembly[guid] = results.CompiledAssembly;
            else
                _assembly.Add(guid, results.CompiledAssembly);

            if (_serviceInterface.ContainsKey(guid))
                _serviceInterface[guid] = _assembly[guid].GetTypes()[0];
            else
                _serviceInterface.Add(guid, _assembly[guid].GetTypes()[0]);
        }

        public static List<string> GetAllServiceMethods(string guid)
        {
            return _serviceInterface[guid].GetMethods().Select(methodInfo => methodInfo.Name).ToList();
        }

        public static ParameterInfo[] GetMethodParameters(string methodName, string guid)
        {
            var serviceClient = GetNewServiceProxyInstance(guid);
            MethodInfo methodInfo = serviceClient.GetType().GetMethod(methodName);
            ParameterInfo[] methodParameters = methodInfo.GetParameters();
            return methodParameters;
        }

        public static Type GetBaseTypeFromCurrentAssembly(string typeName, string guid)
        {
            Type typeByTypeName = PrimitiveTypes.GetTypeByName(typeName);
            if (typeByTypeName != null)
            {
                return typeByTypeName;
            }
            var instance = _assembly[guid].CreateInstance(typeName, true);
            return instance != null ? instance.GetType() : null;
        }

        public static void LoadTest(string loadTestFilePath = null)
        {
            if (string.IsNullOrEmpty(loadTestFilePath))
            {
                loadTestFilePath = Environment.CurrentDirectory + "\\" + ConfigurationManager.AppSettings["loadTestFile"];
            }
            _testConfigurationFile = loadTestFilePath;

            var sr = new StreamReader(_testConfigurationFile);
            var testXml = sr.ReadToEnd();
            sr.Close();

            TestPackage = new Package();

            var xDoc = XDocument.Parse(testXml);

            if (xDoc.Root != null)
            {
                TestPackage.ResultFileName = Convert.ToString(xDoc.Root.Attribute("resultFileName").Value);

                if (xDoc.Root.Attributes("intervalBetweenScenarios").Any())
                {
                    TestPackage.IntervalBetweenScenarios =
                        Convert.ToInt32(xDoc.Root.Attribute("intervalBetweenScenarios").Value);
                }

                TestPackage.Duration = Convert.ToInt32(xDoc.Root.Attribute("duration").Value);
                TestPackage.Clients = Convert.ToInt32(xDoc.Root.Attribute("clients").Value);

                TestPackage.DelayRangeStart = Convert.ToInt32(xDoc.Root.Attribute("delayRangeStart").Value);
                TestPackage.DelayRangeEnd = Convert.ToInt32(xDoc.Root.Attribute("delayRangeEnd").Value);

                TestPackage.ResultFileName = Convert.ToString(xDoc.Root.Attribute("resultFileName").Value);
                if (string.IsNullOrEmpty(TestPackage.ResultFileName))
                {
                    TestPackage.ResultFileName = "PerfResults.txt";
                }

                if (File.Exists(TestPackage.ResultFileName))
                {
                    File.Delete(TestPackage.ResultFileName);
                }


                if (xDoc.Root.Elements("nodes").Any())
                {
                    TestPackage.Nodes = new Nodes { NodeList = new List<string>() };

                    XElement nsE = xDoc.Root.Elements("nodes").ElementAt(0);
                    if (nsE.Attributes("noOfClientsPerNode").Any())
                    {
                        TestPackage.Nodes.NoOfClientsPerNode = Convert.ToInt32(nsE.Attribute("noOfClientsPerNode").Value);
                    }
                    foreach (XElement nE in nsE.Elements("node"))
                    {
                        TestPackage.Nodes.NodeList.Add(nE.Attribute("name").Value);
                    }
                }

                if (xDoc.Root.Elements("scenarios").Any())
                {
                    TestPackage.Scenarios = new List<Scenario>();

                    XElement scenariosE = xDoc.Root.Elements("scenarios").ElementAt(0);
                    foreach (XElement scenarioE in scenariosE.Elements("scenario"))
                    {
                        Scenario scen = new Scenario();
                        foreach (XElement sOrderE in scenarioE.Elements("order"))
                        {
                            ScenarioOrder sorder = new ScenarioOrder
                            {
                                MethodName = Convert.ToString(sOrderE.Attribute("methodName").Value),
                                Order = Convert.ToInt32(sOrderE.Attribute("order").Value),
                                MethodGuid =
                                    sOrderE.Attributes("methodGuid").Any()
                                        ? Convert.ToString(sOrderE.Attribute("methodGuid").Value)
                                        : string.Empty,
                                AssemblyGuid = sOrderE.Attributes("assemblyGuid").Any()
                                        ? Convert.ToString(sOrderE.Attribute("assemblyGuid").Value)
                                        : string.Empty,
                                IsRest = sOrderE.Attributes("isRest").Any() && Convert.ToBoolean(sOrderE.Attribute("isRest").Value)
                            };
                            scen.ScenarioOrder.Add(sorder);
                        }
                        TestPackage.Scenarios.Add(scen);
                    }
                }


                foreach (var suite in xDoc.Root.Elements("testSuite"))
                {
                    var newSuite = new TestSuite();
                    TestPackage.Suites.Add(newSuite);


                    newSuite.Guid = Convert.ToString(suite.Attribute("__guid__").Value);
                    newSuite.ServiceUrl = Convert.ToString(suite.Attribute("serviceUrl").Value);
                    newSuite.BaseUrl = newSuite.ServiceUrl;
                    newSuite.Wsdl = newSuite.ServiceUrl + "?wsdl";

                    if (suite.Attributes("bindingToTest").Any())
                        newSuite.BindingToTest = Convert.ToString(suite.Attribute("bindingToTest").Value);

                    if (suite.Attributes("configuration").Any())
                        newSuite.Configuration = Convert.ToString(suite.Attribute("configuration").Value);



                    foreach (XElement test in suite.Elements("test"))
                    {
                        LoadServiceElement(test, newSuite.Tests, false);
                    }

                    foreach (XElement test in suite.Elements("functionalTest"))
                    {
                        LoadServiceElement(test, newSuite.FunctionalTests, true);
                    }
                    GenerateProxyAssembly(newSuite.Wsdl, newSuite.Guid);
                }

                #region rest api
                foreach (var restNodeX in xDoc.Root.Elements("restApis"))
                {
                    foreach (XElement restMethodX in restNodeX.Elements("restMethod"))
                    {
                        RestMethod restMethod = new RestMethod()
                        {
                            Guid = Convert.ToString(restMethodX.Attribute("__guid__").Value),
                            Url = Convert.ToString(restMethodX.Attribute("url").Value),
                            SelectedHeaderTab = Convert.ToInt32(restMethodX.Attribute("selectedHeaderTab").Value),
                            SelectedPayloadTab = Convert.ToInt32(restMethodX.Attribute("selectedPayloadTab").Value),
                            Headers = (List<KeyValue>)TestHelper.Deserialize(restMethodX.Attribute("headers").Value, typeof(List<KeyValue>)),
                            HeaderText = Convert.ToString(restMethodX.Attribute("headerText").Value),
                            PayloadValues = (List<KeyValue>)TestHelper.Deserialize(restMethodX.Attribute("payload").Value, typeof(List<KeyValue>)),
                            PayloadText = Convert.ToString(restMethodX.Attribute("payloadText").Value),
                            ContentType = Convert.ToString(restMethodX.Attribute("contentType").Value),
                            Type = (RequestType)Enum.Parse(typeof(RequestType), Convert.ToString(restMethodX.Attribute("type").Value)),
                            IsAddedToFunctional = restMethodX.Attributes("isAddedToFunctional").Any() && Convert.ToBoolean(restMethodX.Attribute("isAddedToFunctional").Value),
                            MethodOutput = restMethodX.Attributes("methodOutput").Any() ?
                                Convert.ToString(restMethodX.Attribute("methodOutput").Value)
                                : string.Empty

                        };

                        TestPackage.RestMethods.Add(restMethod);
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Invoke a servie method async
        /// </summary>
        /// <param name="t">Test to run</param>
        /// <param name="guid"></param>
        /// <param name="valueFromScenario">Method value from scenario</param>
        public static void InvokeTest(Common.Test t, string guid, Value valueFromScenario = null)
        {
            bool isMethodAdded = false;
            string token = Guid.NewGuid().ToString();
            try
            {
                var serviceClient = GetNewServiceProxyInstance(guid);
                MethodInfo methodToInvoke = serviceClient.GetType().GetMethod(t.Service.MethodName);
                MethodInfo closeMethod = serviceClient.GetType().GetMethod("Close");
                MethodInfo abortMethod = serviceClient.GetType().GetMethod("Abort");

                InsertNewMethodLog(methodToInvoke.Name, token);
                isMethodAdded = true;

                #region parameter setting section
                List<object> parameters = new List<object>();

                ParameterInfo[] methodParameters = methodToInvoke.GetParameters();
                if (methodParameters.Any())
                {
                    foreach (ParameterInfo mParameter in methodParameters)
                    {
                        var parameterOrder = mParameter.Position;
                        //mParameter.ParameterType
                        Value valToUse;
                        if (valueFromScenario == null)
                        {
                            var r = new Random();
                            var maxValCount = t.Service.Values.ValueList.Count;
                            var indexOfValueToUse = r.Next(0, maxValCount - 1);
                            valToUse = t.Service.Values.ValueList[indexOfValueToUse];
                        }
                        else
                        {
                            valToUse = valueFromScenario;
                        }

                        var propertyValue = (from vp in valToUse.Param
                                             where vp.Order == parameterOrder
                                             select vp).First();

                        GetParameterForServiceMethod(mParameter, propertyValue, parameters, guid);
                    }
                }
                #endregion

                #region call service method
                {
                    UpdateMethodLog(token, methodToInvoke.Name, MethodStatus.PreThread);

                    Task.Run(() =>
                    {
                        if (PerformanceRunToken.IsCancellationRequested)
                        {
                            return;
                        }

                        UpdateMethodLog(token, methodToInvoke.Name, MethodStatus.Started);

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        try
                        {
                            // ReSharper disable once UnusedVariable
                            var returnVal = methodToInvoke.Invoke(serviceClient, parameters.ToArray());
                            closeMethod.Invoke(serviceClient, null);
                            TotalPass++;
                            TotalResponsesRecieved++;
                            long ellapsedTime = sw.ElapsedMilliseconds;
                            UpdateMethodLog(token, methodToInvoke.Name, MethodStatus.Pass, ellapsedTime);
                        }
                        catch (Exception ex)
                        {
                            TotalFail++;
                            TotalResponsesRecieved++;
                            abortMethod.Invoke(serviceClient, null);
                            long ellapsedTime1 = sw.ElapsedMilliseconds;
                            UpdateMethodLog(token, methodToInvoke.Name, MethodStatus.Fail, ellapsedTime1, ex.Message + " -- " + ex.StackTrace);
                        }
                        finally
                        {
                            sw.Stop();
                        }
                    }, PerformanceRunToken);
                }
                #endregion
            }
            catch (Exception er)
            {
                if (isMethodAdded)
                {
                    UpdateMethodLog(token, t.Service.MethodName, MethodStatus.Error, null, er.Message + " -- " + er.StackTrace);
                }
            }
        }

        /// <summary>
        /// Invoke a single method and return serialized output as string
        /// </summary>
        /// <param name="t">Test</param>
        /// <param name="indexOfValueToUse">Index of value to use</param>
        /// <param name="guid"></param>
        /// <param name="input">out param, returns input as serialized string</param>
        /// <returns></returns>
        public static string InvokeFunctionalTest(Common.Test t, int indexOfValueToUse, string guid, out string input)
        {
            var serviceClient = GetNewServiceProxyInstance(guid);
            MethodInfo methodToInvoke = serviceClient.GetType().GetMethod(t.Service.MethodName);
            MethodInfo closeMethod = serviceClient.GetType().GetMethod("Close");
            MethodInfo abortMethod = serviceClient.GetType().GetMethod("Abort");

            #region parameter setting section
            List<object> parameters = new List<object>();

            ParameterInfo[] methodParameters = methodToInvoke.GetParameters();
            if (methodParameters.Any())
            {
                foreach (ParameterInfo mParameter in methodParameters)
                {
                    int parameterOrder = mParameter.Position;
                    //mParameter.ParameterType


                    Value valToUse = t.Service.Values.ValueList[indexOfValueToUse];

                    var propertyValue = (from vp in valToUse.Param
                                         where vp.Order == parameterOrder
                                         select vp).First();

                    GetParameterForServiceMethod(mParameter, propertyValue, parameters, guid);
                }
            }
            #endregion

            input = string.Empty;
            #region call service method
            try
            {
                var returnVal = methodToInvoke.Invoke(serviceClient, parameters.ToArray());
                closeMethod.Invoke(serviceClient, null);
                input = string.Empty;
                foreach (var paramObj in parameters)
                {
                    input += TestHelper.Serialize(paramObj);
                    input += Environment.NewLine;
                }
                return TestHelper.Serialize(returnVal);
            }
            catch (Exception)
            {
                abortMethod.Invoke(serviceClient, null);
            }
            #endregion
            return string.Format("Error occured during executing of method {0}", t.Service.MethodName);
        }

        public static RestMethodResponse InvokeRestApi(RestMethod r, bool trackTime)
        {
            bool isMethodAdded = false;
            string token = Guid.NewGuid().ToString();
            RestMethodResponse webResponse = new RestMethodResponse();
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(r.Url);

                webRequest.ContentType = r.ContentType;
                webRequest.ContentLength = 0;
                webRequest.Method = r.Type.ToString().ToUpper();

                webRequest.AllowAutoRedirect = true;
                //set headers 
                if (r.SelectedHeaderTab == 0)
                {
                    foreach (var kv in r.Headers)
                    {
                        webRequest.Headers.Add(string.Format("{0}:{1}", kv.Key, kv.Value));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(r.HeaderText))
                    {
                        var headers = r.HeaderText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        foreach (var h in headers.Where(h => h.IndexOf(":", StringComparison.Ordinal) > -1))
                        {
                            webRequest.Headers.Add(h);
                        }
                    }
                }
                //set post data
                var postData = r.SelectedPayloadTab == 0
                    ? string.Join("&", r.PayloadValues.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value)).ToArray())
                    : r.PayloadText.Replace(Environment.NewLine, " ");

                if (!string.IsNullOrEmpty(postData) && (r.Type == RequestType.Post || r.Type == RequestType.Put))
                {
                    var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(postData);
                    webRequest.ContentLength = bytes.Length;

                    using (var writeStream = webRequest.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }

                if (trackTime)
                    InsertNewMethodLog(r.Url, token);
                isMethodAdded = true;
                if (trackTime)
                    UpdateMethodLog(token, r.Url, MethodStatus.Started);

                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    using (var response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            webResponse.StatusCode = response.StatusCode;

                            var ellapsedTime2 = sw.ElapsedMilliseconds;
                            if (trackTime)
                                UpdateMethodLog(token, r.Url, MethodStatus.Fail, ellapsedTime2,
                                string.Format("Status Code returned : {0}", response.StatusCode));
                            return webResponse;
                        }
                        // grab the response
                        webResponse.StatusCode = response.StatusCode;
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream == null) return webResponse;
                            using (var reader = new StreamReader(responseStream))
                            {
                                webResponse.Response = reader.ReadToEnd();
                            }
                        }
                        var ellapsedTime = sw.ElapsedMilliseconds;
                        if (trackTime)
                            UpdateMethodLog(token, r.Url, MethodStatus.Pass, ellapsedTime);
                        return webResponse;
                    }
                }
                catch (Exception ex)
                {
                    var ellapsedTime1 = sw.ElapsedMilliseconds;
                    if (trackTime)
                        UpdateMethodLog(token, r.Url, MethodStatus.Fail, ellapsedTime1, ex.Message + " -- " + ex.StackTrace);
                    webResponse.Response = ex.Message;
                    webResponse.StatusCode = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception er)
            {
                if (isMethodAdded && trackTime)
                {
                    UpdateMethodLog(token, r.Url, MethodStatus.Error, null, er.Message + " -- " + er.StackTrace);
                }
                webResponse.Response = er.Message;
                webResponse.StatusCode = HttpStatusCode.InternalServerError;
            }
            return webResponse;
        }
        #endregion

        #region supporting methods to generate parameters

        private static void GetParameterForServiceMethod(ParameterInfo mParameter, ParamValue propertyValue, List<object> parameters, string guid)
        {
            if (!PrimitiveTypes.Test(mParameter.ParameterType))
            {
                if (mParameter.ParameterType.Name.ToLower() == "dictionary`2"
                    && mParameter.ParameterType.IsGenericType
                    && mParameter.ParameterType.GenericTypeArguments != null
                    && mParameter.ParameterType.GenericTypeArguments.Length == 2)
                {
                    var dictionaryObj = GetDictionaryObj(mParameter.ParameterType, propertyValue, guid);
                    parameters.Add(dictionaryObj);
                }
                else if (mParameter.ParameterType.IsArray)
                {
                    int dimensions;
                    Type arrayBaseType;
                    var aOo = GetArrayObj(propertyValue, mParameter.ParameterType, guid, out dimensions, out arrayBaseType);

                    if (dimensions >= 2)
                    {
                        var d = ArrayHelper.GetJaggedArray(dimensions, arrayBaseType, aOo);
                        parameters.Add(d);
                    }
                    else
                    {
                        parameters.Add(aOo);
                    }
                }
                else
                {
                    var oo = _assembly[guid].CreateInstance(mParameter.ParameterType.FullName, true);

                    foreach (PropertyInfo pProperty in mParameter.ParameterType.GetProperties())
                    {
                        //Primitive Type
                        if (propertyValue.Param.ContainsKey(pProperty.Name))
                        {
                            if (PrimitiveTypes.Test(pProperty.PropertyType))
                            {
                                string val = propertyValue.Param[pProperty.Name];
                                val = TestHelper.CheckForSpecialValue(val);
                                if (oo == null) continue;
                                var propertyInfo = oo.GetType().GetProperty(pProperty.Name);
                                SetPrimitiveTypeValue(pProperty, val, propertyInfo, oo);
                            }
                            else
                            {
                                //Non-primitive type
                                GetObject(pProperty, propertyValue, oo, guid);
                            }
                        }
                    }
                    parameters.Add(oo);
                }
            }
            else
            {
                object safeValue = TestHelper.GetSafeValue(mParameter.ParameterType, propertyValue.Param["value"]);
                parameters.Add(safeValue);
                //parameters.Add(Convert.ChangeType(propertyValue.param["value"], mParameter.ParameterType));
            }
        }

        private static void SetPrimitiveTypeValue(PropertyInfo pProperty, string val, PropertyInfo propertyInfo, object oo)
        {
            if (pProperty.PropertyType.IsEnum)
            {
                var enumValue = TestHelper.GetDefaultEnumValue(pProperty.PropertyType, val);
                propertyInfo.SetValue(oo, Convert.ChangeType(enumValue, propertyInfo.PropertyType));
            }
            else
            {
                object safeValue = TestHelper.GetSafeValue(propertyInfo.PropertyType, val);
                propertyInfo.SetValue(oo, safeValue);
            }
        }

        private static Array GetArrayObj(ParamValue propertyValue, Type type, string guid, out int dimensions,
            out Type arrayBaseType)
        {
            dimensions = type.FullName.Split(new[] { "[]" }, StringSplitOptions.None).Length - 1;
            arrayBaseType = type.GetElementType();
            for (int ii = 1; ii < dimensions; ii++)
            {
                arrayBaseType = arrayBaseType.GetElementType();
            }

            var parameterObject = arrayBaseType.MakeArrayType();


            for (int ii = 1; ii < dimensions; ii++)
            {
                parameterObject = parameterObject.MakeArrayType();
            }

            List<int> arrayIndexes = propertyValue.ArrayIndexes.Select(o => o.Int).ToList();

            Array aOo = Array.CreateInstance(arrayBaseType, arrayIndexes.ToArray());

            int i = 0;

            #region traverse array elements

            foreach (var lElement in propertyValue.ArrayElements)
            {
                object arrayElementObj = GetArrayElementObject(arrayBaseType, guid, lElement);

                List<int> indexes = new List<int>();

                for (int ir = 0; ir < arrayIndexes.Count; ir++)
                {
                    #region old logic, check if incorrect delete
                    //int tot = 0;
                    //for (int ri = arrayIndexes.Count - 1; ri > ir; ri--)
                    //{
                    //    tot += arrayIndexes[ri];
                    //}

                    //if (tot == 0)
                    //{
                    //    tot = arrayIndexes[arrayIndexes.Count - 1];
                    //}

                    //if (ir == arrayIndexes.Count - 1)
                    //{
                    //    indexes.Add(i%tot);
                    //}
                    //else
                    //{
                    //    indexes.Add(i/tot);
                    //}
                    #endregion

                    TestHelper.GetArrayIndexesFromLinearIndex(arrayIndexes, ir, indexes, i);
                }

                aOo.SetValue(arrayElementObj, indexes.ToArray());

                i++;
            }

            #endregion

            return aOo;
        }

        private static IDictionary GetDictionaryObj(Type dictionaryType, ParamValue propertyValue, string guid)
        {
            var dictionaryObj = (IDictionary)Activator.CreateInstance(dictionaryType);

            int totalElements = propertyValue.DictionaryLength;
            for (int di = 0; di < totalElements; di++)
            {
                var dkey = GetListDictionaryObject(dictionaryType.GenericTypeArguments[0],
                    propertyValue.DictKeyElements[di], guid);
                var dvalue = GetListDictionaryObject(dictionaryType.GenericTypeArguments[1],
                    propertyValue.DictValueElements[di], guid);

                dictionaryObj.Add(dkey, dvalue);
            }
            return dictionaryObj;
        }

        private static object GetListDictionaryObject(Type baseType, ParamValue propertyValue, string guid)
        {
            if (baseType.IsArray)
            {
                var coPropertyValue = propertyValue.GuidValues[0];

                int dimensions;
                Type arrayBaseType;
                var aOo = GetArrayObj(coPropertyValue, baseType, guid, out dimensions, out arrayBaseType);

                if (dimensions >= 2)
                {
                    var d = ArrayHelper.GetJaggedArray(dimensions, arrayBaseType, aOo);
                    return d;
                }
                return aOo;
            }
            if (baseType.Name.ToLower() == "dictionary`2"
                && baseType.IsGenericType
                && baseType.GenericTypeArguments != null
                && baseType.GenericTypeArguments.Length == 2)
            {
                var dictionaryObj = GetDictionaryObj(baseType, propertyValue, guid);
                return dictionaryObj;
            }
            if (PrimitiveTypes.Test(baseType))
            {
                object safeValue = TestHelper.GetSafeValue(baseType, propertyValue.Param["value"]);
                return safeValue;
            }
            if (!PrimitiveTypes.Test(baseType))
            {
                var oo = _assembly[guid].CreateInstance(baseType.FullName, true); //.Unwrap();

                foreach (PropertyInfo pProperty in baseType.GetProperties())
                {
                    //Primitive Type
                    if (propertyValue.Param.ContainsKey(pProperty.Name))
                    {
                        if (PrimitiveTypes.Test(pProperty.PropertyType))
                        {
                            string val = propertyValue.Param[pProperty.Name];
                            val = TestHelper.CheckForSpecialValue(val);
                            if (oo != null)
                            {
                                PropertyInfo propertyInfo = oo.GetType().GetProperty(pProperty.Name);
                                SetPrimitiveTypeValue(pProperty, val, propertyInfo, oo);
                            }
                        }
                        else
                        {
                            //Non-primitive type
                            GetObject(pProperty, propertyValue, oo, guid);
                        }
                    }
                }
                return oo;
            }
            return GetArrayElementObject(baseType, guid, propertyValue);
        }

        private static object GetArrayElementObject(Type baseType, string guid, ParamValue propertyValue)
        {
            var coo = _assembly[guid].CreateInstance(baseType.FullName, true); //.Unwrap();
            var cooPropertyValue = propertyValue;
            // (from pv in propertyValue.guidValues
            //where pv.guid == cooGuid
            //select pv).First();
            if (PrimitiveTypes.Test(baseType))
            {
                object safeValue = TestHelper.GetSafeValue(baseType, propertyValue.Param["value"]);
                coo = safeValue;
                return coo;
            }
            if (baseType.Name.ToLower() == "dictionary`2"
                              && baseType.IsGenericType
                              && baseType.GenericTypeArguments != null
                              && baseType.GenericTypeArguments.Length == 2)
            {
                var coPropertyValue = propertyValue.GuidValues[0];
                var dictionaryObj = GetDictionaryObj(baseType, coPropertyValue, guid);
                return dictionaryObj;
            }
            foreach (PropertyInfo mProperty in baseType.GetProperties())
            {
                if (mProperty.PropertyType.FullName == "System.Runtime.Serialization.ExtensionDataObject")
                    continue;

                if (mProperty.PropertyType.Name.ToLower() == "dictionary`2"
                              && mProperty.PropertyType.IsGenericType
                              && mProperty.PropertyType.GenericTypeArguments != null
                              && mProperty.PropertyType.GenericTypeArguments.Length == 2)
                {
                    var cooGuid1 = cooPropertyValue.Param[mProperty.Name];
                    var cooPropertyValue1 = (from pv in cooPropertyValue.GuidValues
                                             where pv.Guid == cooGuid1
                                             select pv).First();

                    var dictionaryObj = GetDictionaryObj(mProperty.PropertyType, cooPropertyValue1, guid);
                    mProperty.SetValue(coo, dictionaryObj);
                }
                else if (mProperty.PropertyType.IsArray)
                {
                    var cooGuid1 = cooPropertyValue.Param[mProperty.Name];
                    var cooPropertyValue1 = (from pv in cooPropertyValue.GuidValues
                                             where pv.Guid == cooGuid1
                                             select pv).First();


                    int dimensions;
                    Type arrayBaseType;
                    var aOo = GetArrayObj(cooPropertyValue1, mProperty.PropertyType, guid, out dimensions, out arrayBaseType);

                    if (dimensions >= 2)
                    {
                        var d = ArrayHelper.GetJaggedArray(dimensions, arrayBaseType, aOo);
                        mProperty.SetValue(coo, d);
                    }
                    else
                    {
                        mProperty.SetValue(coo, aOo);
                    }
                }
                else
                {
                    //Primitive Type
                    if (cooPropertyValue.Param.ContainsKey(mProperty.Name))
                    {
                        if (PrimitiveTypes.Test(mProperty.PropertyType))
                        {
                            string val = cooPropertyValue.Param[mProperty.Name];
                            val = TestHelper.CheckForSpecialValue(val);
                            if (coo != null)
                            {
                                PropertyInfo propertyInfo = coo.GetType().GetProperty(mProperty.Name);
                                SetPrimitiveTypeValue(mProperty, val, propertyInfo, coo);
                            }
                        }
                        else
                        {
                            //Non-primitive type
                            GetObject(mProperty, cooPropertyValue, coo, guid);
                        }
                    }
                    //mProperty.SetValue(coo, a_oo);
                }
            }
            return coo;
        }

        private static void GetObject(PropertyInfo pProperty, ParamValue propertyValue, object oo, string guid)
        {
            var coo = _assembly[guid].CreateInstance(pProperty.PropertyType.FullName, true); //.Unwrap();
            var cooGuid = propertyValue.Param[pProperty.Name];
            var cooPropertyValue = (from pv in propertyValue.GuidValues
                                    where pv.Guid == cooGuid
                                    select pv).First();

            foreach (PropertyInfo cpProperty in pProperty.PropertyType.GetProperties())
            {
                if (cpProperty.PropertyType.FullName == "System.Runtime.Serialization.ExtensionDataObject")
                    continue;

                //object oo = null;
                if (cpProperty.PropertyType.Name.ToLower() == "dictionary`2"
                             && cpProperty.PropertyType.IsGenericType
                             && cpProperty.PropertyType.GenericTypeArguments != null
                             && cpProperty.PropertyType.GenericTypeArguments.Length == 2)
                {

                    var dictionaryObj = GetDictionaryObj(cpProperty.PropertyType, propertyValue, guid);
                    cpProperty.SetValue(coo, dictionaryObj);
                }
                else if (cpProperty.PropertyType.IsArray)
                {

                    int dimensions;
                    Type arrayBaseType;
                    var aOo = GetArrayObj(propertyValue, cpProperty.PropertyType, guid, out dimensions, out arrayBaseType);

                    if (coo != null)
                    {
                        PropertyInfo propertyInfo = coo.GetType().GetProperty(cpProperty.Name);

                        if (dimensions >= 2)
                        {
                            var d = ArrayHelper.GetJaggedArray(dimensions, arrayBaseType, aOo);
                            propertyInfo.SetValue(coo, d);
                        }
                        else
                        {
                            propertyInfo.SetValue(coo, aOo);
                        }
                    }
                }
                //Primitive Type
                else if (cooPropertyValue.Param.ContainsKey(cpProperty.Name))
                {
                    if (PrimitiveTypes.Test(cpProperty.PropertyType))
                    {
                        string val = cooPropertyValue.Param[cpProperty.Name];
                        val = TestHelper.CheckForSpecialValue(val);
                        if (coo != null)
                        {
                            PropertyInfo propertyInfo = coo.GetType().GetProperty(cpProperty.Name);
                            SetPrimitiveTypeValue(cpProperty, val, propertyInfo, coo);
                        }
                    }
                    else
                    {
                        //Non-primitive type
                        GetObject(cpProperty, cooPropertyValue, coo, guid);
                    }
                }
            }

            pProperty.SetValue(oo, coo);
        }

        #endregion

        #region suppporting methods to load a test file

        /// <summary>
        /// Load service element from xml
        /// </summary>
        /// <param name="test">test xml node</param>
        /// <param name="testCollection">perf test or functional test</param>
        /// <param name="isFunctional">Is Functional</param>
        private static void LoadServiceElement(XElement test, List<Common.Test> testCollection, bool isFunctional)
        {
            var xElement = test.Element("service");
            if (xElement != null)
            {
                var t = new Common.Test
                {
                    Service = new Service
                    {
                        MethodName = xElement.Attribute("methodName").Value,
                        IsAsync = Convert.ToBoolean(xElement.Attribute("isAsync").Value),
                        Parameters = new Parameter()
                    }
                };


                if (xElement.HasElements)
                {
                    var valuesElement = xElement.Element("values");
                    if (valuesElement != null)
                    {
                        t.Service.Values = new Values { ValueList = new List<Value>() };
                        List<XElement> valElements = valuesElement.Elements("value").ToList();
                        foreach (XElement val in valElements)
                        {
                            Value v = new Value
                            {
                                Param = new List<ParamValue>(),
                                Guid = val.Attributes("__guid__").Any() ? val.Attribute("__guid__").Value : string.Empty
                            };
                            if (isFunctional)
                            {
                                v.MethodOutput = val.Attribute("methodOutput").Value;
                            }
                            List<XElement> valParams = val.Elements("param").ToList();
                            foreach (XElement vp in valParams)
                            {
                                ParamValue pv = new ParamValue
                                {
                                    Param = new Dictionary<string, string>(),
                                    Order = Convert.ToInt32(vp.Attribute("order").Value)
                                };

                                if (vp.Attributes("isDictionary").ToList().Count > 0)
                                {
                                    PopulateDictionaryElement(pv, vp);
                                }
                                else if (vp.Attributes("isArray").ToList().Count > 0)
                                {
                                    PopulateArrayElement(pv, vp);
                                }
                                else
                                {
                                    PopulateComplexElement(vp, pv);
                                }
                                v.Param.Add(pv);
                            }
                            t.Service.Values.ValueList.Add(v);
                        }
                    }
                }

                testCollection.Add(t);
            }
        }

        private static void PopulateComplexElement(XElement vp, ParamValue pv)
        {
            foreach (XAttribute attr in vp.Attributes().ToList())
            {
                GetElementValue(vp, pv, attr);
            }
        }

        private static void GetElementValue(XElement vp, ParamValue pv, XAttribute attr)
        {
            string localName = TestHelper.DecodeAttributeName(attr.Name.LocalName);
            pv.Param.Add(localName, attr.Value);
            PopulateGuidValues(vp, pv, attr);
        }

        private static void PopulateArrayElement(ParamValue pv, XElement vp)
        {
            pv.IsArray = true;
            string indexesStr = vp.Attribute("arrayIndexes").Value;
            pv.ArrayIndexes = new List<IntWrappper>();
            foreach (string istr in indexesStr.Split(','))
            {
                pv.ArrayIndexes.Add(new IntWrappper
                {
                    Int = Convert.ToInt32(istr)
                });
            }
            foreach (XElement xArrayElement in vp.Elements("param"))
            {
                PopulateArrayElementValues(xArrayElement, pv);
            }
        }

        private static void PopulateDictionaryElement(ParamValue pv, XElement vp)
        {
            pv.IsDictionary = true;
            pv.DictionaryLength = Convert.ToInt32(vp.Attribute("dictionaryLength").Value);
            int i = 0;
            foreach (XElement xArrayElement in vp.Elements("param"))
            {
                PopulateDictElementValues(xArrayElement, pv, i <= pv.DictionaryLength / 2);
                i++;
            }
        }

        private static ParamValue PopulateListTypeElement(XElement vp)
        {
            ParamValue pv = new ParamValue { Param = new Dictionary<string, string>() };

            if (vp.Attributes("isDictionary").ToList().Count > 0)
            {
                PopulateDictionaryElement(pv, vp);
            }
            else if (vp.Attributes("isArray").ToList().Count > 0)
            {
                PopulateArrayElement(pv, vp);
            }
            else
            {
                PopulateComplexElement(vp, pv);
            }
            return pv;
        }

        private static void PopulateDictElementValues(XElement vp, ParamValue parentvalue, bool isKey)
        {
            var pv = PopulateListTypeElement(vp);

            if (isKey)
            {
                parentvalue.DictKeyElements.Add(pv);
            }
            else
            {
                parentvalue.DictValueElements.Add(pv);
            }
        }

        private static void PopulateArrayElementValues(XElement vp, ParamValue parentvalue)
        {
            var pv = PopulateListTypeElement(vp);

            parentvalue.ArrayElements.Add(pv);
        }


        private static void PopulateGuidValues(XElement vp, ParamValue pv, XAttribute attr)
        {
            string resultString = Regex.Replace(attr.Value,
                                 @"\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b",
                                 "'$0'",
                                 RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(resultString))
            {
                var vpplist = vp.Elements("param").Where(vp1 => vp1.Attribute("__guid__") != null && vp1.Attribute("__guid__").Value == attr.Value).ToList();
                if (vpplist.Count > 0)
                {
                    ParamValue vpp = new ParamValue
                    {
                        Guid = attr.Value,
                        Param = new Dictionary<string, string>()
                    };

                    if (vpplist[0].Attributes("isDictionary").ToList().Count > 0)
                    {
                        PopulateDictionaryElement(vpp, vpplist[0]);
                    }
                    else if (vpplist[0].Attributes("isArray").ToList().Count > 0)
                    {
                        PopulateArrayElement(vpp, vpplist[0]);
                    }
                    else
                    {
                        foreach (XAttribute attr1 in vpplist[0].Attributes().ToList())
                        {
                            if (attr1.Name.LocalName == "__guid__")
                                continue;

                            GetElementValue(vpplist[0], vpp, attr1);
                        }
                    }
                    pv.GuidValues.Add(vpp);
                }
            }
        }

        #endregion
    }
}
