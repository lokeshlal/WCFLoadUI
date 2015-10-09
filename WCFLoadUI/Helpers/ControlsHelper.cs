#region File Information/History
// <copyright file="ControlsHelper.cs" project="WCFLoadUI" >
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Common;
using WCFLoad.Helper;
using WCFLoadUI.TypeToBind;
using WCFLoadUI.ViewModels;
using Test = WCFLoad.TestEngine;
using Common.Infrastructure.Entities;

// ReSharper disable RedundantNameQualifier

namespace WCFLoadUI.Helpers
{
    public static class ControlsHelper
    {
        #region method select and value select
        public static void SetCollectionOnValue(Value selectedValueObj, ParameterInfo[] parameters, ObservableCollection<ControlView> controlViewBindingObject, string guid)
        {
            if (parameters.Any())
            {
                foreach (ParameterInfo mParameter in parameters)
                {
                    int parameterOrder = mParameter.Position;
                    var paramValue = (from p in selectedValueObj.Param
                                      where p.Order == parameterOrder
                                      select p).First();

                    GetParameter(controlViewBindingObject, mParameter, parameterOrder, guid, paramValue);
                }
            }
        }

        public static void SetEmptyCollectionOnMethodSelect(ParameterInfo[] parameters, ObservableCollection<ControlView> controlViewBindingObject, string guid)
        {
            if (parameters.Any())
            {
                foreach (ParameterInfo mParameter in parameters)
                {
                    int parameterOrder = mParameter.Position;
                    //mParameter.ParameterType

                    GetParameter(controlViewBindingObject, mParameter, parameterOrder, guid);
                }
            }
        }

        private static ControlView GetTypeDetails(Type propertyInfo, string guid, ParamValue paramValue = null)
        {
            bool isNullableType = Nullable.GetUnderlyingType(propertyInfo) != null;

            if (
                propertyInfo.Name.ToLower() == "dictionary`2"
                        && propertyInfo.IsGenericType
                        && propertyInfo.GenericTypeArguments != null
                        && propertyInfo.GenericTypeArguments.Length == 2)
            {
                var newParamValues = GetNewParamValues(propertyInfo.Name, paramValue);
                var arrayObject = GetTypeDetailsDictionaryObject(propertyInfo, propertyInfo.Name, newParamValues, guid);

                return arrayObject;
            }
            //for list
            else if (propertyInfo.IsArray)
            {
                int dimensions = propertyInfo.FullName.Split(new[] { "[]" }, StringSplitOptions.None).Length - 1;
                Type arrayBaseType = propertyInfo.GetElementType();
                for (int ii = 1; ii < dimensions; ii++)
                {
                    arrayBaseType = arrayBaseType.GetElementType();
                }

                var newParamValues = GetNewParamValues(propertyInfo.Name, paramValue);
                var arrayObject = GetTypeDetailsArrayObject(propertyInfo.Name, arrayBaseType, newParamValues, dimensions, guid);
                return arrayObject;
            }
            else if (propertyInfo.IsPrimitive
                // ReSharper disable once PossibleMistakenCallToGetType.2
                || propertyInfo.GetType().IsPrimitive
                || propertyInfo.Name.ToLower() == "string"
                || (isNullableType && PrimitiveTypes.Test(Nullable.GetUnderlyingType(propertyInfo)))
                || PrimitiveTypes.Test(propertyInfo)
                )
            {

                string value = string.Empty;
                if (paramValue != null)
                {
                    value = paramValue.Param.ContainsKey(propertyInfo.Name) ? paramValue.Param[propertyInfo.Name] : string.Empty;
                    if (string.IsNullOrEmpty(value))
                    {
                        value = paramValue.Param.ContainsKey("value") ? paramValue.Param["value"] : string.Empty;
                    }
                }

                var cv = GetTypeDetailPrimitiveObject(propertyInfo, propertyInfo.Name, isNullableType, value, guid);
                return cv;
            }
            else
            {
                ParamValue newParamValues = null;
                if (paramValue != null)
                {
                    newParamValues = paramValue;
                }
                ControlView cv1 = GetTypeDetailComplexObject(propertyInfo, guid, newParamValues);
                return cv1;
            }
        }

        private static ControlView GetPropertyDetails(PropertyInfo propertyInfo, string guid, ParamValue paramValue = null)
        {
            bool isNullableType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;


            if (
               propertyInfo.PropertyType.Name.ToLower() == "dictionary`2"
                       && propertyInfo.PropertyType.IsGenericType
                       && propertyInfo.PropertyType.GenericTypeArguments != null
                       && propertyInfo.PropertyType.GenericTypeArguments.Length == 2)
            {
                var newParamValues = GetNewParamValues(propertyInfo.Name, paramValue);


                var arrayObject = GetTypeDetailsDictionaryObject(propertyInfo.PropertyType, propertyInfo.Name, newParamValues, guid);
                return arrayObject;
            }
            //for list
            else if (propertyInfo.PropertyType.IsArray)
            {
                int dimensions = propertyInfo.PropertyType.FullName.Split(new[] { "[]" }, StringSplitOptions.None).Length - 1;
                Type arrayBaseType = propertyInfo.PropertyType.GetElementType();
                for (int ii = 1; ii < dimensions; ii++)
                {
                    arrayBaseType = arrayBaseType.GetElementType();
                }

                var newParamValues = GetNewParamValues(propertyInfo.Name, paramValue);
                var arrayObject = GetTypeDetailsArrayObject(propertyInfo.Name, arrayBaseType, newParamValues, dimensions, guid);
                return arrayObject;
            }
            else if (propertyInfo.PropertyType.IsPrimitive
                || propertyInfo.GetType().IsPrimitive
                || propertyInfo.PropertyType.Name.ToLower() == "string"
                || (isNullableType && PrimitiveTypes.Test(Nullable.GetUnderlyingType(propertyInfo.PropertyType)))
                || PrimitiveTypes.Test(propertyInfo.PropertyType)
                )
            {
                string value = string.Empty;
                if (paramValue != null)
                {
                    value = paramValue.Param.ContainsKey(propertyInfo.Name) ? paramValue.Param[propertyInfo.Name] : string.Empty;
                }


                var cv = GetTypeDetailPrimitiveObject(propertyInfo.PropertyType, propertyInfo.Name, isNullableType, value, guid);
                return cv;
            }
            else
            {
                var newParamValues = GetNewParamValues(propertyInfo.Name, paramValue);
                ControlView cv1 = GetTypeDetailComplexObject(propertyInfo.PropertyType, guid, newParamValues, propertyInfo.Name);
                return cv1;
            }
        }



        #region Get parameter object
        /// <summary>
        /// Get the parameter object
        /// </summary>
        /// <param name="controlViewBindingObject">controls list to populate</param>
        /// <param name="mParameter">parameter info</param>
        /// <param name="parameterOrder">parameter order in method</param>
        /// <param name="paramValue">param value from xml</param>
        private static void GetParameter(ObservableCollection<ControlView> controlViewBindingObject, ParameterInfo mParameter,
            int parameterOrder, string guid, ParamValue paramValue = null)
        {
            //for dictionary
            if (mParameter.ParameterType.Name.ToLower() == "dictionary`2"
                && mParameter.ParameterType.IsGenericType
                && mParameter.ParameterType.GenericTypeArguments != null
                && mParameter.ParameterType.GenericTypeArguments.Length == 2)
            {
                var arrayObject = GetDictionaryObject(mParameter, parameterOrder, guid, paramValue);
                controlViewBindingObject.Add(arrayObject);
            }
            //for list
            else if (mParameter.ParameterType.IsArray)
            {
                var arrayObject = GetArrayObject(mParameter, parameterOrder, guid, paramValue);
                controlViewBindingObject.Add(arrayObject);
            }
            else if (!mParameter.ParameterType.IsPrimitive
                     && !PrimitiveTypes.Test(mParameter.ParameterType))
            {
                var complexObject = GetComplexObject(mParameter, parameterOrder, guid, paramValue);
                controlViewBindingObject.Add(complexObject);
            }
            else
            {
                var primitiveObject = GetPrimitiveObject(mParameter, parameterOrder, guid, paramValue);
                controlViewBindingObject.Add(primitiveObject);
            }
        }


        private static ParamValue GetNewParamValues(string name, ParamValue paramValue)
        {
            ParamValue newParamValues = null;
            if (paramValue != null)
            {
                string objectGuid = paramValue.Param.ContainsKey(name)
                    ? paramValue.Param[name]
                    : string.Empty;
                if (!string.IsNullOrEmpty(objectGuid))
                {
                    var newParamValuesList = (from pv in paramValue.GuidValues
                                              where pv.Guid == objectGuid
                                              select pv).ToList();
                    if (newParamValuesList.Count > 0)
                    {
                        newParamValues = newParamValuesList[0];
                    }
                }
            }
            return newParamValues;
        }

        /// <summary>
        /// Creates Primitive object
        /// </summary>
        /// <param name="mParameter">Parameter info</param>
        /// <param name="parameterOrder">parameter order</param>
        /// <param name="guid"></param>
        /// <param name="paramValue">param value object from project</param>
        /// <returns></returns>
        private static ControlView GetPrimitiveObject(ParameterInfo mParameter, int parameterOrder, string guid, ParamValue paramValue = null)
        {
            var primitiveObject = new ControlView()
            {
                IsPrimitive = true,
                PControlView = new PrimitiveControlViewModel()
                {
                    FieldName = mParameter.Name,
                    FieldValue = paramValue != null ?
                        (paramValue.Param.ContainsKey("value") ? paramValue.Param["value"] : string.Empty) : string.Empty,
                    Order = parameterOrder,
                    FieldType = mParameter.ParameterType,
                    AssemblyGuid = guid
                }
            };
            return primitiveObject;
        }


        private static ControlView GetTypeDetailPrimitiveObject(Type fieldType, string fieldName, bool isNullableType, string value, string guid)
        {
            ControlView cv = new ControlView { IsPrimitive = true };
            if (isNullableType)
            {
                cv.PControlView = new PrimitiveControlViewModel()
                {
                    FieldName = fieldName,
                    FieldValue = value,
                    FieldType = Nullable.GetUnderlyingType(fieldType),
                    AssemblyGuid = guid
                };
            }
            else
            {
                cv.PControlView = new PrimitiveControlViewModel()
                {
                    FieldName = fieldName,
                    FieldValue = value,
                    FieldType = fieldType,
                    AssemblyGuid = guid
                };
            }
            return cv;
        }

        /// <summary>
        /// Creates Complex object
        /// </summary>
        /// <param name="mParameter">Parameter info</param>
        /// <param name="parameterOrder">parameter order</param>
        /// <param name="guid"></param>
        /// <param name="paramValue">param value object from project</param>
        /// <returns></returns>
        private static ControlView GetComplexObject(ParameterInfo mParameter, int parameterOrder, string guid, ParamValue paramValue = null)
        {
            ObservableCollection<ControlView> parameterPropertiesNew = new ObservableCollection<ControlView>();
            foreach (PropertyInfo pProperty in mParameter.ParameterType.GetProperties())
            {
                if (pProperty.PropertyType.FullName == "System.Runtime.Serialization.ExtensionDataObject")
                    continue;
                parameterPropertiesNew.Add(GetPropertyDetails(pProperty, guid, paramValue));
            }

            var complexObject = new ControlView()
            {
                IsPrimitive = false,
                CControlView = new ComplexControlViewModel()
                {
                    FieldName = mParameter.Name,
                    Properties = parameterPropertiesNew,
                    Order = parameterOrder
                }
            };
            return complexObject;
        }

        private static ControlView GetTypeDetailComplexObject(Type propertyInfo, string guid, ParamValue newParamValues = null, string fieldName = null)
        {
            ObservableCollection<ControlView> parameterPropertiesNew = new ObservableCollection<ControlView>();
            foreach (PropertyInfo pProperty in propertyInfo.GetProperties())
            {
                if (pProperty.PropertyType.FullName == "System.Runtime.Serialization.ExtensionDataObject")
                    continue;
                parameterPropertiesNew.Add(GetPropertyDetails(pProperty, guid, newParamValues));
            }
            ControlView cv1 = new ControlView()
            {
                IsPrimitive = false,
                CControlView = new ComplexControlViewModel()
                {
                    FieldName = !string.IsNullOrEmpty(fieldName) ? fieldName : propertyInfo.Name,
                    Properties = parameterPropertiesNew
                }
            };
            return cv1;
        }

        /// <summary>
        /// Creates Dictionary object
        /// </summary>
        /// <param name="mParameter">Parameter info</param>
        /// <param name="parameterOrder">parameter order</param>
        /// <param name="paramValue">param value object from project</param>
        /// <returns></returns>
        private static ControlView GetDictionaryObject(ParameterInfo mParameter, int parameterOrder, string guid, ParamValue paramValue = null)
        {
            ControlView arrayObject = new ControlView
            {
                IsDictionary = true,
                DControlView = new DictionartControlViewModel
                {
                    FieldName = mParameter.Name,
                    FieldType = mParameter.ParameterType.GenericTypeArguments[0],
                    FieldValueType = mParameter.ParameterType.GenericTypeArguments[1],
                    Order = parameterOrder,
                    DictionaryItemsCount = paramValue != null ? paramValue.DictionaryLength : 0,
                    BaseTypeProperties = GetTypeDetails(mParameter.ParameterType.GenericTypeArguments[0], guid),
                    BaseValueTypeProperties =
                        GetTypeDetails(mParameter.ParameterType.GenericTypeArguments[1], guid),
                    AssemblyGuid = guid
                }
            };

            for (int i = 0; i < arrayObject.DControlView.DictionaryItemsCount; i++)
            {
                arrayObject.DControlView.Properties.Add(GetTypeDetails(mParameter.ParameterType.GenericTypeArguments[0], guid,
                    paramValue != null ? paramValue.DictKeyElements[i] : null));
                arrayObject.DControlView.PropertiesValue.Add(GetTypeDetails(mParameter.ParameterType.GenericTypeArguments[1], guid,
                    paramValue != null ? paramValue.DictValueElements[i] : null));
            }
            return arrayObject;
        }

        private static ControlView GetTypeDetailsDictionaryObject(Type propertyInfo, string fieldName, ParamValue newParamValues, string guid)
        {
            ControlView arrayObject = new ControlView
            {
                IsDictionary = true,
                DControlView = new DictionartControlViewModel
                {
                    FieldName = fieldName,
                    FieldType = propertyInfo.GenericTypeArguments[0],
                    FieldValueType = propertyInfo.GenericTypeArguments[1],
                    AssemblyGuid = guid
                }
            };

            if (newParamValues != null)
            {
                arrayObject.DControlView.DictionaryItemsCount = newParamValues.DictionaryLength;

                for (int i = 0; i < arrayObject.DControlView.DictionaryItemsCount; i++)
                {
                    arrayObject.DControlView.Properties.Add(GetTypeDetails(propertyInfo.GenericTypeArguments[0], guid,
                        newParamValues.DictKeyElements[i]));
                    arrayObject.DControlView.PropertiesValue.Add(GetTypeDetails(propertyInfo.GenericTypeArguments[1], guid,
                        newParamValues.DictValueElements[i]));
                }
            }
            else
            {
                arrayObject.DControlView.BaseTypeProperties = GetTypeDetails(propertyInfo.GenericTypeArguments[0], guid);
                arrayObject.DControlView.BaseValueTypeProperties = GetTypeDetails(propertyInfo.GenericTypeArguments[1], guid);
            }
            return arrayObject;
        }

        /// <summary>
        /// Creates Dictionary object
        /// </summary>
        /// <param name="mParameter">Parameter info</param>
        /// <param name="parameterOrder">parameter order</param>
        /// <param name="paramValue">param value object from project</param>
        /// <returns></returns>
        private static ControlView GetArrayObject(ParameterInfo mParameter, int parameterOrder, string guid, ParamValue paramValue = null)
        {
            int dimensions = mParameter.ParameterType.FullName.Split(new[] { "[]" }, StringSplitOptions.None).Length - 1;
            Type arrayBaseType = mParameter.ParameterType.GetElementType();
            for (int ii = 1; ii < dimensions; ii++)
            {
                arrayBaseType = arrayBaseType.GetElementType();
            }

            ControlView arrayObject = new ControlView
            {
                IsList = true,
                LControlView = new ListControlViewModel
                {
                    FieldName = mParameter.Name,
                    FieldType = arrayBaseType,
                    Order = parameterOrder,
                    BaseTypeProperties = GetTypeDetails(arrayBaseType, guid),
                    AssemblyGuid = guid
                }
            };

            //for existing item, i.e. for value select from values list
            if (paramValue != null)
            {
                arrayObject.LControlView.ArrayIndexes = new BindingList<IntWrappper>(paramValue.ArrayIndexes);
                int totalElementsInList = arrayObject.LControlView.ArrayIndexes.Aggregate(1, (current, iw) => current * iw.Int);

                for (int i = 0; i < totalElementsInList; i++)
                {
                    arrayObject.LControlView.Properties.Add(GetTypeDetails(arrayBaseType, guid, paramValue.ArrayElements[i]));
                }
            }
            else
            {
                //for new object creation
                for (int ai = 0; ai < dimensions; ai++)
                {
                    arrayObject.LControlView.ArrayIndexes.Add(new IntWrappper()
                    {
                        Int = 0
                    });
                }
            }
            return arrayObject;
        }

        private static ControlView GetTypeDetailsArrayObject(string fieldName, Type arrayBaseType, ParamValue newParamValues,
            int dimensions, string guid)
        {
            ControlView arrayObject = new ControlView
            {
                IsList = true,
                LControlView = new ListControlViewModel
                {
                    FieldName = fieldName,
                    FieldType = arrayBaseType,
                    BaseTypeProperties = GetTypeDetails(arrayBaseType, guid),
                    AssemblyGuid = guid
                }
            };

            if (newParamValues != null)
            {
                arrayObject.LControlView.ArrayIndexes = new BindingList<IntWrappper>(newParamValues.ArrayIndexes);
                int totalElementsInList = arrayObject.LControlView.ArrayIndexes.Aggregate(1, (current, iw) => current * iw.Int);

                for (int i = 0; i < totalElementsInList; i++)
                {
                    arrayObject.LControlView.Properties.Add(GetTypeDetails(arrayBaseType, guid, newParamValues.ArrayElements[i]));
                }
            }
            else
            {
                for (int ai = 0; ai < dimensions; ai++)
                {
                    arrayObject.LControlView.ArrayIndexes.Add(new IntWrappper()
                    {
                        Int = 0
                    });
                }
            }
            return arrayObject;
        }

        #endregion
        #endregion

        #region save test file

        public static XDocument GenerateTestSuiteXmlDocument(XDocument xmlDoc)
        {
            if (xmlDoc.Root == null) return null;

            xmlDoc.Root.SetAttributeValue("duration", Test.TestPackage.Duration);
            xmlDoc.Root.SetAttributeValue("clients", Test.TestPackage.Clients);
            xmlDoc.Root.SetAttributeValue("delayRangeStart", Test.TestPackage.DelayRangeStart);
            xmlDoc.Root.SetAttributeValue("delayRangeEnd", Test.TestPackage.DelayRangeEnd);
            xmlDoc.Root.SetAttributeValue("resultFileName", Test.TestPackage.ResultFileName);
            xmlDoc.Root.SetAttributeValue("intervalBetweenScenarios", Test.TestPackage.IntervalBetweenScenarios);

            #region scenario
            if (Test.TestPackage.Scenarios.Count > 0)
            {
                XElement scenriosE = new XElement("scenarios");

                foreach (var sc in Test.TestPackage.Scenarios)
                {
                    XElement scenE = new XElement("scenario");
                    foreach (var so in sc.ScenarioOrder)
                    {
                        XElement scenOrderE = new XElement("order");
                        scenOrderE.SetAttributeValue("methodGuid", so.MethodGuid);
                        scenOrderE.SetAttributeValue("assemblyGuid", so.AssemblyGuid);
                        scenOrderE.SetAttributeValue("methodName", so.MethodName);
                        scenOrderE.SetAttributeValue("isRest", so.IsRest);
                        scenOrderE.SetAttributeValue("order", so.Order);
                        scenE.Add(scenOrderE);
                    }
                    scenriosE.Add(scenE);
                }
                xmlDoc.Root.Add(scenriosE);
            }
            #endregion
            #region nodes
            if (Test.TestPackage.Nodes.NodeList.Count > 0)
            {
                XElement nodesEl = new XElement("nodes");

                nodesEl.SetAttributeValue("noOfClientsPerNode", Test.TestPackage.Nodes.NoOfClientsPerNode);

                foreach (var n in Test.TestPackage.Nodes.NodeList)
                {
                    XElement nE = new XElement("node");
                    nE.SetAttributeValue("name", n);
                    nodesEl.Add(nE);
                }
                xmlDoc.Root.Add(nodesEl);
            }
            #endregion


            foreach (var suite in Test.TestPackage.Suites)
            {
                XElement testSuite = new XElement("testSuite");

                testSuite.SetAttributeValue("__guid__", suite.Guid);
                testSuite.SetAttributeValue("serviceUrl", suite.ServiceUrl);
                testSuite.SetAttributeValue("configuration", "");
                testSuite.SetAttributeValue("bindingToTest", suite.BindingToTest);

                #region
                foreach (var t in suite.Tests)
                {
                    XElement test = new XElement("test");
                    XElement service = new XElement("service");
                    service.SetAttributeValue("methodName", t.Service.MethodName);
                    service.SetAttributeValue("isAsync", "false");
                    test.Add(service);

                    XElement valuesNode = new XElement("values");
                    service.Add(valuesNode);

                    foreach (var v in t.Service.Values.ValueList)
                    {
                        XElement valueNode = new XElement("value");
                        valueNode.SetAttributeValue("__guid__", v.Guid);
                        valuesNode.Add(valueNode);
                        foreach (var p in v.Param)
                        {
                            XElement paramNode = new XElement("param");
                            paramNode.SetAttributeValue("order", p.Order);

                            if (p.IsDictionary)
                            {
                                paramNode.SetAttributeValue("isDictionary", "true");
                                paramNode.SetAttributeValue("dictionaryLength", p.DictionaryLength);
                                foreach (ParamValue pl in p.DictKeyElements)
                                {
                                    BuildArrayElementXml(pl, paramNode);
                                }
                                foreach (ParamValue pl in p.DictValueElements)
                                {
                                    BuildArrayElementXml(pl, paramNode);
                                }
                            }
                            else if (p.IsArray)
                            {
                                paramNode.SetAttributeValue("isArray", "true");
                                paramNode.SetAttributeValue("arrayIndexes",
                                    string.Join(",", p.ArrayIndexes.Select(o => o.Int).ToArray()));
                                foreach (ParamValue pl in p.ArrayElements)
                                {
                                    BuildArrayElementXml(pl, paramNode);
                                }
                            }
                            else
                            {
                                foreach (var item in p.Param)
                                {
                                    var newKey = TestHelper.EncodeAttributeName(item.Key);
                                    paramNode.SetAttributeValue(newKey, item.Value);
                                }

                                if (p.GuidValues.Count > 0)
                                {
                                    BuildGuidXmlNodes(p, paramNode);
                                }
                            }

                            valueNode.Add(paramNode);
                        }
                    }
                    testSuite.Add(test);
                }


                #endregion

                #region functional test

                foreach (var t in suite.FunctionalTests)
                {
                    XElement test = new XElement("functionalTest");
                    XElement service = new XElement("service");
                    service.SetAttributeValue("methodName", t.Service.MethodName);
                    service.SetAttributeValue("isAsync", "false");
                    test.Add(service);

                    XElement valuesNode = new XElement("values");
                    service.Add(valuesNode);

                    foreach (var v in t.Service.Values.ValueList)
                    {
                        XElement valueNode = new XElement("value");
                        valueNode.SetAttributeValue("__guid__", v.Guid);
                        valueNode.SetAttributeValue("methodOutput", v.MethodOutput);
                        valuesNode.Add(valueNode);
                        foreach (var p in v.Param)
                        {
                            XElement paramNode = new XElement("param");
                            paramNode.SetAttributeValue("order", p.Order);

                            if (p.IsDictionary)
                            {
                                paramNode.SetAttributeValue("isDictionary", "true");
                                paramNode.SetAttributeValue("dictionaryLength", p.DictionaryLength);
                                foreach (ParamValue pl in p.DictKeyElements)
                                {
                                    BuildArrayElementXml(pl, paramNode);
                                }
                                foreach (ParamValue pl in p.DictValueElements)
                                {
                                    BuildArrayElementXml(pl, paramNode);
                                }
                            }
                            else if (p.IsArray)
                            {
                                paramNode.SetAttributeValue("isArray", "true");
                                paramNode.SetAttributeValue("arrayIndexes",
                                    string.Join(",", p.ArrayIndexes.Select(o => o.Int).ToArray()));
                                foreach (ParamValue pl in p.ArrayElements)
                                {
                                    BuildArrayElementXml(pl, paramNode);
                                }
                            }
                            else
                            {
                                foreach (var item in p.Param)
                                {
                                    var newKey = TestHelper.EncodeAttributeName(item.Key);
                                    paramNode.SetAttributeValue(newKey, item.Value);
                                }

                                if (p.GuidValues.Count > 0)
                                {
                                    BuildGuidXmlNodes(p, paramNode);
                                }
                            }

                            valueNode.Add(paramNode);
                        }
                    }

                    testSuite.Add(test);
                }
                #endregion

                xmlDoc.Root.Add(testSuite);
            }

            #region rest urls

            if (Test.TestPackage.RestMethods.Count > 0)
            {
                var restNode = new XElement("restApis");
                foreach (var restMethod in Test.TestPackage.RestMethods)
                {
                    var restMethodX = new XElement("restMethod");
                    restMethodX.SetAttributeValue("__guid__", restMethod.Guid);
                    restMethodX.SetAttributeValue("isRest", "true");
                    restMethodX.SetAttributeValue("url", restMethod.Url);
                    restMethodX.SetAttributeValue("selectedHeaderTab", restMethod.SelectedHeaderTab);
                    restMethodX.SetAttributeValue("selectedPayloadTab", restMethod.SelectedPayloadTab);
                    restMethodX.SetAttributeValue("headers",  TestHelper.Serialize(restMethod.Headers));
                    restMethodX.SetAttributeValue("headerText", restMethod.HeaderText);
                    restMethodX.SetAttributeValue("payload", TestHelper.Serialize(restMethod.PayloadValues));
                    restMethodX.SetAttributeValue("payloadText", restMethod.PayloadText);
                    restMethodX.SetAttributeValue("type", restMethod.Type.ToString());
                    restMethodX.SetAttributeValue("contentType", restMethod.ContentType.ToString());
                    restMethodX.SetAttributeValue("isAddedToFunctional", restMethod.IsAddedToFunctional);
                    restMethodX.SetAttributeValue("methodOutput", restMethod.MethodOutput);
                    restNode.Add(restMethodX);
                }
                xmlDoc.Root.Add(restNode);
            }

            #endregion
            return xmlDoc;
        }

        private static void BuildArrayElementXml(ParamValue p, XElement valueNode)
        {
            XElement paramNode = new XElement("param");
            if (p.IsArray)
            {
                paramNode.SetAttributeValue("isDictionary", "true");
                paramNode.SetAttributeValue("dictionaryLength", p.DictionaryLength);

                foreach (ParamValue pl in p.DictKeyElements)
                {
                    BuildArrayElementXml(pl, paramNode);
                }
                foreach (ParamValue pl in p.DictValueElements)
                {
                    BuildArrayElementXml(pl, paramNode);
                }
            }
            else if (p.IsArray)
            {
                paramNode.SetAttributeValue("isArray", "true");
                paramNode.SetAttributeValue("arrayIndexes", string.Join(",", p.ArrayIndexes.Select(o => o.Int).ToArray()));
                foreach (ParamValue pl in p.ArrayElements)
                {
                    BuildArrayElementXml(pl, paramNode);
                }
            }
            else
            {
                foreach (var item in p.Param)
                {

                    var newKey = TestHelper.EncodeAttributeName(item.Key);
                    paramNode.SetAttributeValue(newKey, item.Value);
                }

                if (p.GuidValues.Count > 0)
                {
                    BuildGuidXmlNodes(p, paramNode);
                }
            }

            valueNode.Add(paramNode);
        }

        private static void BuildGuidXmlNodes(ParamValue p, XElement paramNode)
        {
            foreach (var pg in p.GuidValues)
            {
                var childParamNode = new XElement("param");
                childParamNode.SetAttributeValue("__guid__", pg.Guid);
                if (pg.IsDictionary)
                {
                    childParamNode.SetAttributeValue("isDictionary", "true");
                    childParamNode.SetAttributeValue("dictionaryLength", pg.DictionaryLength);
                    foreach (ParamValue pl in pg.DictKeyElements)
                    {
                        BuildArrayElementXml(pl, childParamNode);
                    }
                    foreach (ParamValue pl in pg.DictValueElements)
                    {
                        BuildArrayElementXml(pl, childParamNode);
                    }
                }
                else if (pg.IsArray)
                {
                    childParamNode.SetAttributeValue("isArray", "true");
                    childParamNode.SetAttributeValue("arrayIndexes", string.Join(",", pg.ArrayIndexes.Select(o => o.Int).ToArray()));
                    foreach (ParamValue pl in pg.ArrayElements)
                    {
                        BuildArrayElementXml(pl, childParamNode);
                    }
                }
                else
                {
                    foreach (var pg1 in pg.Param)
                    {
                        var newKey = TestHelper.EncodeAttributeName(pg1.Key);
                        childParamNode.SetAttributeValue(newKey, pg1.Value);
                    }
                    if (pg.GuidValues.Count > 0)
                    {
                        BuildGuidXmlNodes(pg, childParamNode);
                    }
                }
                paramNode.Add(childParamNode);
            }
        }


        #endregion

        #region save value to in-memory object

        public static Value GetParameterValueFromControlViewList(ObservableCollection<ControlView> controlViewBindingObject)
        {
            List<ControlView> valuesToSave = controlViewBindingObject.ToList();

            Value valueNode = new Value { Param = new List<ParamValue>() };
            foreach (ControlView cv in valuesToSave)
            {
                ParamValue paramValue = new ParamValue { Param = new Dictionary<string, string>() };
                if (cv.IsDictionary)
                {
                    paramValue.Order = cv.DControlView.Order;
                    GetDictionaryObjectIm(cv, paramValue);
                }
                else if (cv.IsList)
                {
                    paramValue.Order = cv.LControlView.Order;
                    GetArrayObjectIm(cv, paramValue);
                }
                else if (cv.IsPrimitive)
                {
                    paramValue.Order = cv.PControlView.Order;
                    paramValue.Param.Add("value", cv.PControlView.FieldValue);
                }
                else
                {
                    paramValue.Order = cv.CControlView.Order;
                    foreach (var property in cv.CControlView.Properties)
                    {
                        if (property.IsPrimitive)
                        {
                            paramValue.Param.Add(property.PControlView.FieldName, property.PControlView.FieldValue);
                        }
                        else
                        {
                            GetComplexItemIm(paramValue, property);
                        }
                    }
                }

                valueNode.Param.Add(paramValue);
            }
            return valueNode;
        }

        private static ParamValue CreateArrayItem(ControlView cv, Guid? gf = null)
        {
            ParamValue paramValue = new ParamValue
            {
                ArrayElements = new List<ParamValue>(),
                Param = new Dictionary<string, string>()
            };
            if (gf != null)
            {
                paramValue.Guid = gf.Value.ToString();
            }
            if (cv.IsDictionary)
            {
                paramValue.Order = cv.DControlView.Order;

                GetDictinaryItemIm(cv, paramValue);
            }
            else if (cv.IsList)
            {
                paramValue.Order = cv.LControlView.Order;

                GetArrayItemIm(cv, paramValue);
            }
            else if (cv.IsPrimitive)
            {
                paramValue.Order = cv.PControlView.Order;
                paramValue.Param.Add("value", cv.PControlView.FieldValue);
            }
            else
            {
                paramValue.Order = cv.CControlView.Order;
                foreach (var property in cv.CControlView.Properties)
                {
                    if (property.IsDictionary)
                    {
                        GetDictinaryItemIm(property, paramValue);
                    }
                    else if (property.IsList)
                    {
                        GetArrayItemIm(property, paramValue);
                    }
                    else if (property.IsPrimitive)
                    {
                        paramValue.Param.Add(property.PControlView.FieldName, property.PControlView.FieldValue);
                    }
                    else
                    {
                        GetComplexItemIm(paramValue, property);
                    }
                }
            }
            return paramValue;
        }

        private static void GetComplexItemIm(ParamValue paramValue, ControlView property)
        {
            Guid g = Guid.NewGuid();
            paramValue.Param.Add(property.CControlView.FieldName, g.ToString());
            paramValue.GuidValues.Add(WriteComplexType(property.CControlView.Properties.ToList(), g));
        }

        private static void GetArrayItemIm(ControlView cv, ParamValue paramValue)
        {
            Guid g = Guid.NewGuid();
            paramValue.Param.Add(cv.LControlView.FieldName, g.ToString());

            ParamValue paramValueList = new ParamValue { Param = new Dictionary<string, string>() };
            GetArrayObjectIm(cv, paramValueList, g);

            paramValue.GuidValues.Add(paramValueList);
        }

        private static void GetDictinaryItemIm(ControlView cv, ParamValue paramValue)
        {
            Guid g = Guid.NewGuid();
            paramValue.Param.Add(cv.DControlView.FieldName, g.ToString());

            ParamValue paramValueList = new ParamValue { Param = new Dictionary<string, string>() };
            GetDictionaryObjectIm(cv, paramValueList, g);

            paramValue.GuidValues.Add(paramValueList);
        }

        private static void GetDictionaryObjectIm(ControlView cv, ParamValue paramValue, Guid? g = null)
        {
            paramValue.DictionaryLength = cv.DControlView.DictionaryItemsCount;
            paramValue.IsDictionary = true;
            if (g != null)
            {
                paramValue.Guid = g.Value.ToString();
            }

            foreach (ControlView eleCv in cv.DControlView.Properties)
            {
                paramValue.DictKeyElements.Add(CreateArrayItem(eleCv));
            }
            foreach (ControlView eleCv in cv.DControlView.PropertiesValue)
            {
                paramValue.DictValueElements.Add(CreateArrayItem(eleCv));
            }
        }

        private static void GetArrayObjectIm(ControlView cv, ParamValue paramValue, Guid? g = null)
        {
            paramValue.ArrayIndexes = cv.LControlView.ArrayIndexes.ToList();
            paramValue.IsArray = true;
            if (g != null)
            {
                paramValue.Guid = g.Value.ToString();
            }
            paramValue.ArrayElements = new List<ParamValue>();
            foreach (ControlView eleCv in cv.LControlView.Properties)
            {
                paramValue.ArrayElements.Add(CreateArrayItem(eleCv));
            }
        }

        private static ParamValue WriteComplexType(List<ControlView> property, Guid g)
        {
            ParamValue paramValue = new ParamValue
            {
                Guid = g.ToString(),
                Param = new Dictionary<string, string>()
            };
            foreach (ControlView cv in property)
            {
                if (cv.IsDictionary)
                {
                    paramValue.Order = cv.DControlView.Order;
                    GetDictinaryItemIm(cv, paramValue);

                }
                else if (cv.IsList)
                {
                    paramValue.Order = cv.LControlView.Order;
                    GetArrayItemIm(cv, paramValue);

                }
                else if (cv.IsPrimitive)
                {
                    paramValue.Order = cv.PControlView.Order;
                    paramValue.Param.Add(cv.PControlView.FieldName, cv.PControlView.FieldValue);
                }
                else
                {
                    GetComplexItemIm(paramValue, cv);
                }
            }
            return paramValue;
        }

        #endregion
    }
}
