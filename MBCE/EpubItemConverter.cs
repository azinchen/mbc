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
using System.IO;

namespace MobiBatchConverter.Engine
{
    /// <summary>
    /// Represents methods for converting ePub source file item
    /// </summary>
    public class EpubItemConverter : ItemConverter
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
        public EpubItemConverter(string inputFileName, string outputFileName, bool deleteInputFile, bool rewriteOutputFile,
            int compressionLevel, bool verbose)
            : base(inputFileName, outputFileName, deleteInputFile, rewriteOutputFile, compressionLevel, verbose)
        {
        }

        #endregion

        /// <summary>
        /// Convert ePub source file
        /// </summary>
        protected override void DoConvert()
        {
            RunKindlegen(LocalInputFile);
        }

        /// <summary>
        /// ePub file extension of source file
        /// </summary>
        protected override string FileExt
        {
            get { return ".epub"; }
        }  
    }
}
