using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Entities;

namespace Common
{
    [Serializable]
    public class TestSuite //: ISerializable
    {
        [NonSerialized]
        private Dictionary<string, List<EndpointsForContractsClass>> endpointsForContracts;

        private SerializationInfo serializationInfo;

        /// <summary>
        /// Initializes the deafult instance of TestSuite.
        /// </summary>
        public TestSuite()
        {
            FunctionalTests = new List<Test>();
            Tests = new List<Test>();
            //EndpointsForContracts = new Dictionary<string, List<ServiceEndpoint>>();
            endpointsForContracts = new Dictionary<string, List<EndpointsForContractsClass>>();
            EndPointType = new Dictionary<string, string>();
            EndPoints = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes the object of TestSuite class.
        /// </summary>
        /// <param name="info">SerializationInfo object</param>
        /// <param name="context">Optional StreamingContext</param>
        public TestSuite(SerializationInfo info, Nullable<StreamingContext> context = null)
        {
            info.CheckForNotNull(() => info);

            this.serializationInfo = info;

            // remove it after complete code refactoring. 
            // Note: constructor shouldn't be doing operations which can throw exceptions other than argument checking.
            // expose a public method to initialize the object.
            this.Initialize();
        }

        public string Guid { get; set; }
        public List<Test> Tests { get; set; }
        public List<Test> FunctionalTests { get; set; }
        public string Wsdl { get; set; }
        public string BaseUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string Configuration { get; set; }
        public string BindingToTest { get; set; }
        public Dictionary<string, string> EndPoints { get; set; }
        public Dictionary<string, string> EndPointType { get; set; }

        //public Dictionary<string, List<ServiceEndpoint>> EndpointsForContracts { get; set; }

        [IgnoreDataMember]
        public Dictionary<string, List<EndpointsForContractsClass>> EndpointsForContracts
        {
            get
            {
                return endpointsForContracts;
            }
            set { endpointsForContracts = value; }
        }

        public void ImportObjectData(SerializationInfo info, Nullable<StreamingContext> context = null)
        {
            info.CheckForNotNull(() => info, "Serialization info should not be null.");

            info.AddValue("EndPoints", EndPoints);
            info.AddValue("EndPointType", EndPointType);
            //info.AddValue("EndpointsForContracts", EndpointsForContracts);
            info.AddValue("Guid", Guid);
            info.AddValue("Tests", Tests);
            info.AddValue("FunctionalTests", FunctionalTests);
            info.AddValue("Wsdl", Wsdl);
            info.AddValue("BaseUrl", BaseUrl);
            info.AddValue("ServiceUrl", ServiceUrl);
            info.AddValue("Configuration", Configuration);
            info.AddValue("BindingToTest", BindingToTest);
        }

        private void Initialize()
        {
            EndPoints = (Dictionary<string, string>)this.serializationInfo.GetValue("EndPoints", typeof(Dictionary<string, string>));
            EndPointType = (Dictionary<string, string>)this.serializationInfo.GetValue("EndPointType", typeof(Dictionary<string, string>));
            //EndpointsForContracts = (Dictionary<string, List<ServiceEndpoint>>)info.GetValue("EndpointsForContracts", typeof(Dictionary<string, List<ServiceEndpoint>>));
            EndpointsForContracts = (Dictionary<string, List<EndpointsForContractsClass>>)this.serializationInfo.GetValue("EndpointsForContracts", typeof(Dictionary<string, List<EndpointsForContractsClass>>));
            Guid = (string)this.serializationInfo.GetValue("Guid", typeof(string));
            Tests = (List<Test>)this.serializationInfo.GetValue("Tests", typeof(List<Test>));
            FunctionalTests = (List<Test>)this.serializationInfo.GetValue("FunctionalTests", typeof(List<Test>));
            Wsdl = (string)this.serializationInfo.GetValue("Wsdl", typeof(string));
            BaseUrl = (string)this.serializationInfo.GetValue("BaseUrl", typeof(string));
            ServiceUrl = (string)this.serializationInfo.GetValue("ServiceUrl", typeof(string));
            Configuration = (string)this.serializationInfo.GetValue("Configuration", typeof(string));
            BindingToTest = (string)this.serializationInfo.GetValue("BindingToTest", typeof(string));
        }
    }
}
