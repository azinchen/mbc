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
using System.Reflection;
using System.Threading;
using System.IO.Compression;

namespace MobiBatchConverter.Engine
{
    /// <summary>
    /// Represents methods for converting source file item
    /// </summary>
    public abstract class ItemConverter
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
        protected ItemConverter(string inputFileName, string outputFileName, bool deleteInputFile, bool rewriteOutputFile,
            int compressionLevel, bool verbose)
        {
            _inputFileName = inputFileName;
            _outputFileName = outputFileName;
            _deleteInputFile = deleteInputFile;
            _rewriteOutputFile = rewriteOutputFile;
            _compressionLevel = compressionLevel;
            _verbose = verbose;

            _workingDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_workingDir);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ItemConverter()
        {
            Directory.Delete(WorkingDir, true);

            if ((_deleteInputFile == true) && (Converted == true))
            {
                File.Delete(InputFile);
            }
        }

        #endregion

        #region Delegates/events

        /// <summary>
        /// Conversion complete delegate
        /// </summary>
        /// <param name="inFile">Source file name</param>
        /// <param name="outFile">Destionation file name</param>
        /// <param name="kindlegenOutput">kindlegen console output</param>
        /// <param name="conversionError">Error message if conversion fails</param>
        public delegate void ConvertCompleteHandler(string inFile, string outFile, string kindlegenOutput, string conversionError);

        /// <summary>
        /// Conversion complete event
        /// </summary>
        public static event ConvertCompleteHandler ConvertCompleteEvent;

        #endregion

        #region Protected properties

        /// <summary>
        /// temp working directory
        /// </summary>
        protected string WorkingDir
        {
            get { return _workingDir; }
        }

        /// <summary>
        /// Source file name in temp working dir
        /// </summary>
        protected string LocalInputFile
        {
            get { return Path.Combine(WorkingDir, "book" + FileExt); }
        }

        /// <summary>
        /// File name of converted file in temp working dir
        /// </summary>
        protected string LocalOutputFile
        {
            get { return Path.Combine(WorkingDir, "book.mobi"); }
        }

        /// <summary>
        /// Abstract property, get source file extension
        /// </summary>
        protected abstract string FileExt
        {
            get;
        }  

        #endregion

        #region Private properties

        /// <summary>
        /// True if destination file is created
        /// </summary>
        private bool Converted
        {
            get { return File.Exists(OutputFile); }
        }

        /// <summary>
        /// Source file name
        /// </summary>
        private string InputFile
        {
            get { return _inputFileName; }
        }

        /// <summary>
        /// Destination file name
        /// </summary>
        private string OutputFile
        {
            get { return _outputFileName; }
        }

        /// <summary>
        /// Rewrite output
        /// </summary>
        private bool RewriteOutput
        {
            get { return _rewriteOutputFile; }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// kindlegen file name
        /// </summary>
        private const string _kindlegen = "kindlegen.exe";
        /// <summary>
        /// Temp working directory name
        /// </summary>
        private readonly string _workingDir = string.Empty;
        /// <summary>
        /// Source file name
        /// </summary>
        private readonly string _inputFileName;
        /// <summary>
        /// Destination file name
        /// </summary>
        private readonly string _outputFileName;
        /// <summary>
        /// Source file name in temp working dir
        /// </summary>
        private string _localInputFileName = string.Empty;
        /// <summary>
        /// Delete source file
        /// </summary>
        private readonly bool _deleteInputFile;
        /// <summary>
        /// kindlegen exit code
        /// </summary>
        private int _kindlegenExitCode = -1;
        /// <summary>
        /// Rewrite destination file
        /// </summary>
        private readonly bool _rewriteOutputFile;
        /// <summary>
        /// Compression level for kindlegen 
        /// </summary>
        private readonly int _compressionLevel = 1;
        /// <summary>
        /// Verbose output of kindlegen
        /// </summary>
        private readonly bool _verbose = false;
        /// <summary>
        /// kindlegen console output
        /// </summary>
        private string _kindlegenOutput = string.Empty;

        #endregion

        /// <summary>
        /// Run kindlegen for specific file
        /// </summary>
        /// <param name="inFile">Source file name</param>
        protected void RunKindlegen(string inFile)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = _kindlegen;

                var commandLine = string.Empty;

                if (_verbose == true)
                {
                    commandLine = "-verbose ";
                }

                commandLine += "-c" + _compressionLevel + " \"" + inFile + "\" -o \"" + Path.GetFileName(LocalOutputFile) + "\"";

                process.StartInfo.Arguments = commandLine;

                if (process.Start() == true)
                {
                    process.WaitForExit();
                    _kindlegenOutput = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    _kindlegenExitCode = process.ExitCode;
                }
            }
        }

        /// <summary>
        /// Copy converted file to destination directory
        /// </summary>
        private void CopyConvertedFile()
        {
            var outputExists = File.Exists(OutputFile);

            if ((File.Exists(LocalOutputFile) == true) &&
                ((outputExists == false) || ((outputExists == true) && (RewriteOutput == true))))
            {
                var outDir = Path.GetDirectoryName(OutputFile);

                if (Directory.Exists(outDir) == false)
                {
                    Directory.CreateDirectory(outDir);
                }

                File.Copy(LocalOutputFile, OutputFile, true);
            }
        }

        /// <summary>
        /// Copy source file to temp working directory
        /// </summary>
        private void CopyInputFile()
        {
            if (string.Equals(Path.GetExtension(InputFile), ".zip", StringComparison.OrdinalIgnoreCase) == true)
            {
                using (var zip = ZipFile.OpenRead(InputFile))
                {
                    foreach (var item in zip.Entries)
                    {
                        if (string.Equals(Path.GetExtension(item.FullName), FileExt, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            item.ExtractToFile(LocalInputFile);

                            break;
                        }
                    }
                }
            }
            else
            {
                File.Copy(InputFile, LocalInputFile);
            }
        }

        /// <summary>
        /// Convert source file and copy converted file to destination
        /// </summary>
        public void Convert()
        {
            var catchMessage = string.Empty;

            if ((RewriteOutput == true) || (File.Exists(OutputFile) == false))
            {
                try
                {
                    CopyInputFile();
                    DoConvert();
                    CopyConvertedFile();
                }
                catch (Exception e)
                {
                    catchMessage = e.Message;
                }

            }

            if (ConvertCompleteEvent != null)
            {
                ConvertCompleteEvent(InputFile, OutputFile, _kindlegenOutput, catchMessage);
            }
        }

        /// <summary>
        /// Abstract method for converting specific dource file
        /// </summary>
        protected abstract void DoConvert();
    }
}
