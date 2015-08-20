#region File Information/History
// <copyright file="FunctionResultDetailViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.IO;
using System.Text;
using System.Xml;
using WCFLoadUI.Base;

namespace WCFLoadUI.ViewModels
{
    public class FunctionResultDetailViewModel : BaseViewModel
    {
        #region private fields
        private const string WindowTitleDefault = "Functional Test Details";
        private string _windowTitle = WindowTitleDefault;
        private string _expected = string.Empty;
        private string _expected1 = string.Empty;
        private string _actual = string.Empty;
        private string _input = string.Empty;
        private string _passFailText = string.Empty;
        private bool _passFail;
        private int _differenceIndex = -1;
        #endregion

        #region constructor
        public FunctionResultDetailViewModel() { }
        public FunctionResultDetailViewModel(string actualValue, string expectedValue, bool passFailValue, string inputValue)
        {
            Actual = FormatXml(actualValue);
            Expected = FormatXml(expectedValue);
            PassFail = passFailValue;
            PassFailText = PassFail ? "Pass" : "Fail";
            Input = FormatXml(inputValue);
            if (!PassFail)
            {
                for (int i = 0; i < Expected.Length; i++)
                {
                    if (i < Actual.Length)
                    {
                        if (Actual[i] != Expected[i])
                        {
                            DifferenceIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        DifferenceIndex = i;
                        break;
                    }
                }
                if (DifferenceIndex == -1)
                {
                    DifferenceIndex = 0;
                }

                Expected1 = Expected.Substring(DifferenceIndex);
                Expected = Expected.Substring(0, DifferenceIndex);
            }
        }
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

        public string PassFailText
        {
            get { return _passFailText; }
            set
            {
                _passFailText = value;
                NotifyOfPropertyChange(() => PassFailText);
            }
        }
        public string Expected
        {
            get { return _expected; }
            set
            {
                _expected = value;
                NotifyOfPropertyChange(() => Expected);
            }
        }
        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                NotifyOfPropertyChange(() => Input);
            }
        }
        public string Expected1
        {
            get { return _expected1; }
            set
            {
                _expected1 = value;
                NotifyOfPropertyChange(() => Expected1);
            }
        }
        public string Actual
        {
            get { return _actual; }
            set
            {
                _actual = value;
                NotifyOfPropertyChange(() => Actual);
            }
        }
        public bool PassFail
        {
            get { return _passFail; }
            set
            {
                _passFail = value;
                NotifyOfPropertyChange(() => PassFail);
            }
        }

        public int DifferenceIndex
        {
            get { return _differenceIndex; }
            set
            {
                _differenceIndex = value;
                NotifyOfPropertyChange(() => DifferenceIndex);
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Formats the xml string into xml format
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private static String FormatXml(String xml)
        {
            string result;

            var mStream = new MemoryStream();
            var writer = new XmlTextWriter(mStream, Encoding.Unicode);
            var document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                var sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                var formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                result = xml;
            }

            try
            {
                mStream.Close();
                if (writer.WriteState != WriteState.Closed)
                    writer.Close();
            }
            catch
            {
                // ignored
            }
            return result;
        }
        #endregion
    }
}
