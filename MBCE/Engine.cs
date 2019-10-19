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
using System.Collections.Concurrent;
using System.Threading;

namespace MobiBatchConverter.Engine
{
    /// <summary>
    /// Implements multithreading conversions
    /// </summary>
    public class Engine
    {
        #region Constructors/destructor

        /// <summary>
        /// Initialize conversion engine
        /// </summary>
        /// <param name="deleteInputFile">Delete input files if conversion success</param>
        /// <param name="rewriteOutputFile">Overwrite output files</param>
        /// <param name="compressionLevel">Compression level for kindlegen</param>
        /// <param name="verbose">Verbose output of kindlegen</param>
        public Engine(bool deleteInputFile, bool rewriteOutputFile, int compressionLevel, bool verbose)
        {
            _deleteInputFile = deleteInputFile;
            _rewriteOutputFile = rewriteOutputFile;
            _compressionLevel = compressionLevel;
            _verbose = verbose;

            ItemConverter.ConvertCompleteEvent += new ItemConverter.ConvertCompleteHandler(OnConvertComplete);
        }

        /// <summary>
        /// Conversion engine destructor
        /// </summary>
        ~Engine()
        {
            ItemConverter.ConvertCompleteEvent -= OnConvertComplete;
        }

        #endregion

        #region Delegates/events

        /// <summary>
        /// Conversion start delegate
        /// </summary>
        /// <param name="name">Source file</param>
        public delegate void ConvertStartHandler(string name);

        /// <summary>
        /// Conversion complete delegate
        /// </summary>
        /// <param name="inFile">Source file name</param>
        /// <param name="outFile">Destionation file name</param>
        /// <param name="kindlegenOutput">kindlegen console output</param>
        /// <param name="conversionError">Error message if conversion fails</param>
        public delegate void ConvertCompleteHandler(
            string inFile, string outFile, string kindlegenOutput, string conversionError);

        /// <summary>
        /// Conversion start event
        /// </summary>
        public event ConvertStartHandler ConvertStartEvent;

        /// <summary>
        /// Conversion complete event
        /// </summary>
        public event ConvertCompleteHandler ConvertCompleteEvent;

        #endregion

        #region Private fields

        /// <summary>
        /// Delete input files
        /// </summary>
        private bool _deleteInputFile = false;
        /// <summary>
        /// Overwrite output
        /// </summary>
        private bool _rewriteOutputFile = false;
        /// <summary>
        /// Compression level for kindlegen
        /// </summary>
        private int _compressionLevel = 1;
        /// <summary>
        /// Verbose output for kindlegen
        /// </summary>
        private bool _verbose = false;
        /// <summary>
        /// Container of conversion tasks
        /// </summary>
        private ConcurrentBag<Task> _convertTasks = new ConcurrentBag<Task>();

        #endregion

        /// <summary>
        /// Scan source directory and run conversions task. Directory structure of source will be preserved in destination.
        /// </summary>
        /// <param name="inDir">Source directory of e-books</param>
        /// <param name="outDir">Destination directory</param>
        public void Convert(string inDir, string outDir)
        {
            ScanDirsAndRunConvert(inDir, outDir);

            WaitTillTasksEnd();
        }

        /// <summary>
        /// Run conversion tasks of source files
        /// </summary>
        /// <param name="inFiles">Array of source files</param>
        /// <param name="outDir">Destination directory</param>
        public void Convert(string[] inFiles, string outDir)
        {
            Parallel.ForEach(inFiles, item =>
            {
                ItemConverter convertItem = CreateItemComverter(item, outDir);
                StartConvertTask(item, convertItem);
            });

            WaitTillTasksEnd();
        }

        /// <summary>
        /// Wait until conversion tasks are end
        /// </summary>
        private void WaitTillTasksEnd()
        {
            if (_convertTasks.Count != 0)
            {
                Task.WaitAll(_convertTasks.ToArray());
            }
        }

        /// <summary>
        /// Recurcively scan source directory for source files, create and run conversion task
        /// </summary>
        /// <param name="inDir">Source directory</param>
        /// <param name="outDir">Destination directory</param>
        private void ScanDirsAndRunConvert(string inDir, string outDir)
        {
            Parallel.ForEach(Directory.GetFiles(inDir), item =>
            {
                StartConvertTask(item, CreateItemComverter(item, outDir));
            });

            Parallel.ForEach(Directory.EnumerateDirectories(inDir), item =>
            {
                ScanDirsAndRunConvert(item, Path.Combine(outDir, Path.GetFileName(item)));
            });
        }

        /// <summary>
        /// Event handler when conversion task ends
        /// </summary>
        /// <param name="inFile">Source file name</param>
        /// <param name="outFile">Converted file name</param>
        /// <param name="kindlegenOutput">kindlegen console output</param>
        /// <param name="conversionError">Error message if task fails</param>
        private void OnConvertComplete(string inFile, string outFile, string kindlegenOutput, string conversionError)
        {
            if (ConvertCompleteEvent != null)
            {
                ConvertCompleteEvent(inFile, outFile, kindlegenOutput, conversionError);
            }
        }

        /// <summary>
        /// Check source file for e-book and create conversion object
        /// </summary>
        /// <param name="inFile">Source file name</param>
        /// <param name="outDir">Destination directory</param>
        /// <returns>Conversion object or null if source file is not e-book</returns>
        private ItemConverter CreateItemComverter(string inFile, string outDir)
        {
            ItemConverter convertItem = null;

            if (Path.GetExtension(inFile).Equals(".epub", StringComparison.OrdinalIgnoreCase) == true)
            {
                // .epub e-book file is discovered
                var outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(inFile) + ".mobi");

                convertItem =
                    new EpubItemConverter(inFile, outFile, _deleteInputFile, _rewriteOutputFile, _compressionLevel, _verbose);
            }
            else if ((string.Equals(Path.GetExtension(inFile), ".zip", StringComparison.OrdinalIgnoreCase) == true) &&
                     (string.Equals(Path.GetExtension(
                         Path.GetFileNameWithoutExtension(inFile)), ".epub", StringComparison.OrdinalIgnoreCase) == true))
            {
                // .epub e-book file in zip archive is discovered
                var outFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(inFile)) + ".mobi";
                var outFile = Path.Combine(outDir, outFileName);

                convertItem =
                    new EpubItemConverter(inFile, outFile, _deleteInputFile, _rewriteOutputFile, _compressionLevel, _verbose);
            }
            else if (string.Equals(Path.GetExtension(inFile), ".fb2", StringComparison.OrdinalIgnoreCase) == true)
            {
                // .fb2 e-book file is discovered
                var outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(inFile) + ".mobi");

                convertItem =
                    new Fb2ItemConverter(inFile, outFile, _deleteInputFile, _rewriteOutputFile, _compressionLevel, _verbose);
            }
            else if ((string.Equals(Path.GetExtension(inFile), ".zip", StringComparison.OrdinalIgnoreCase) == true) &&
                     (string.Equals(Path.GetExtension
                     (Path.GetFileNameWithoutExtension(inFile)), ".fb2", StringComparison.OrdinalIgnoreCase) == true))
            {
                // .fb2 e-book file in zip archive is discovered
                var outFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(inFile)) + ".mobi";
                var outFile = Path.Combine(outDir, outFileName);

                convertItem =
                    new Fb2ItemConverter(inFile, outFile, _deleteInputFile, _rewriteOutputFile, _compressionLevel, _verbose);
            }

            return convertItem;
        }

        /// <summary>
        /// Start conversion task, place it to tasks container and raise event
        /// </summary>
        /// <param name="inFile">Source file name</param>
        /// <param name="convertItem">Conversion object which is used for task</param>
        private void StartConvertTask(string inFile, ItemConverter convertItem)
        {
            if (convertItem != null)
            {
                if (ConvertStartEvent != null)
                {
                    ConvertStartEvent(inFile);
                }

                _convertTasks.Add(Task.Run(() => convertItem.Convert()));
            }
        }
    }
}
