// MBCCmd Mobi Batch Converter http://mbc.codeplex.com
//
// The MIT License (MIT)
//
// Copyright (c) 2016 Alexander Zinchenko (alexander@zinchenko.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Reflection;

namespace MobiBatchConverter.Engine
{
    /// <summary>
    /// Represents methods for converting fb2 source file item
    /// </summary>
    public class Fb2ItemConverter : ItemConverter
    {
        #region Constructors/destructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputFileName">Source file name</param>
        /// <param name="outputFileName">Destination file name</param>
        /// <param name="deleteInputFile">Delete source file</param>
        /// <param name="rewriteOutputFile">Rewrite destination file</param>
        /// <param name="compressionLevel"> Compression level for kindlegen</param>
        /// <param name="verbose">Verbose output of kindlegen</param>
        public Fb2ItemConverter(string inputFileName, string outputFileName, bool deleteInputFile, bool rewriteOutputFile,
            int compressionLevel, bool verbose)
            : base(inputFileName, outputFileName, deleteInputFile, rewriteOutputFile, compressionLevel, verbose)
        {
        }

        #endregion

        /// <summary>
        /// Convert fb2 source file
        /// </summary>
        protected override void DoConvert()
        {
            var transformedOpfFileName = Path.Combine(WorkingDir, Path.GetFileNameWithoutExtension(LocalOutputFile) + ".opf");

            SaveImages();

            Transform(MBCE.fb2xsl.fb2_2_xhtml, Path.Combine(WorkingDir, "index.html"));
            Transform(MBCE.fb2xsl.fb2_2_opf, transformedOpfFileName);
            Transform(MBCE.fb2xsl.fb2_2_ncx, Path.Combine(WorkingDir, "book.ncx"));

            RunKindlegen(transformedOpfFileName);
        }

        /// <summary>
        /// Extract and save images from fb2 source file
        /// </summary>
        private void SaveImages()
        {
            var dd = new XmlDocument();

            dd.Load(LocalInputFile);

            for (XmlNode bin = dd["FictionBook"]["binary"]; bin != null; bin = bin.NextSibling)
            {
                using (var fs = new FileStream(Path.Combine(WorkingDir, bin.Attributes["id"].InnerText), FileMode.Create))
                using (var w = new BinaryWriter(fs))
                {
                    w.Write(System.Convert.FromBase64String(bin.InnerText));
                }
            }
        }

        /// <summary>
        /// Transform source file using xsl and save transformed file
        /// </summary>
        /// <param name="xsl">xsl rules</param>
        /// <param name="name">File name of transformed file</param>
        private void Transform(string xsl, string name)
        {
            using (var reader = new XmlTextReader(LocalInputFile))
            using (var writer = new XmlTextWriter(name, null))
            {
                var xslt = new XslCompiledTransform();
                    
                using (var strReader = new StringReader(xsl))
                using (var xmlReader = new XmlTextReader(strReader))
                {
                    xslt.Load(xmlReader);
                }

                xslt.Transform(reader, null, writer, null);
            }
        }

        /// <summary>
        /// fb2 file extension of source file
        /// </summary>
        protected override string FileExt
        {
            get { return ".fb2"; }
        }
    }
}
