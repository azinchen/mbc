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
using System.Threading;
using System.IO;

namespace MobiBatchConverter.CmdTool
{
    /// <summary>
    /// Command line converter entry
    /// </summary>
    class CommandLineConverter
    {
        #region Constructors/destructor

        /// <summary>
        /// Command line converter entry constructor
        /// </summary>
        /// <param name="args">Command line parameters</param>
        public CommandLineConverter(string[] args)
        {
            _args = args;
        }

        /// <summary>
        /// Command line converter entry destructor
        /// </summary>
        ~CommandLineConverter()
        {
            if (_logFile != null)
            {
                _logFile.Close();
                _logFile = null;
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Compression level
        /// </summary>
        private int _compressionLevel = 0;
        /// <summary>
        /// Paths array from command line parameters
        /// </summary>
        private string[] _pathArgs = null;
        /// <summary>
        /// Show help
        /// </summary>
        private bool _showHelp = false;
        /// <summary>
        /// Rewrite output
        /// </summary>
        private bool _rewriteOutput = false;
        /// <summary>
        /// Delete source files
        /// </summary>
        private bool _deleteInput = false;
        /// <summary>
        /// Verbose output of kindlegen
        /// </summary>
        private bool _kindlegenVerbose = false;
        /// <summary>
        /// Log file name
        /// </summary>
        private string _logFileName = string.Empty;
        /// <summary>
        /// File stream of log file
        /// </summary>
        StreamWriter _logFile = null;
        /// <summary>
        /// Log all
        /// </summary>
        private bool _logAll = false;
        /// <summary>
        /// Command line parameters array
        /// </summary>
        private string[] _args;
        /// <summary>
        /// Num of converted files
        /// </summary>
        private ulong _completedFileNum = 0;
        /// <summary>
        /// Total amount of source files
        /// </summary>
        private ulong _totalFileNum = 0;
        /// <summary>
        /// Sum of converted file sizes
        /// </summary>
        private long _completedFileSize = 0;
        /// <summary>
        /// Total size of source files
        /// </summary>
        private long _totalFileSize = 0;
        /// <summary>
        /// Source file names and sizes map
        /// </summary>
        private Dictionary<string, long> _fileSizes = new Dictionary<string, long>();
        /// <summary>
        /// List of unconverted files
        /// </summary>
        private List<string> _unconvertedFile = new List<string>();
        /// <summary>
        /// List of kindlegen output of unconverted files
        /// </summary>
        private List<string> _unconvertedOutput = new List<string>();
        /// <summary>
        /// List of error messages of unconverted files
        /// </summary>
        private List<string> _conversionError = new List<string>();
        /// <summary>
        /// Lock for private counters, lists and arrays
        /// </summary>
        private static ReaderWriterLockSlim _fileNumsLock = new ReaderWriterLockSlim();
        /// <summary>
        /// Start conversion time
        /// </summary>
        DateTime _startTime;

        #endregion

        /// <summary>
        /// Entry point of command line converter
        /// </summary>
        public void Start()
        {
            ShowCopyright();

            if (ParseCommandLine() == false)
            {
                Console.WriteLine("Wrong program options.");
                Console.WriteLine("");

                ShowUsage();
            }
            else if (_showHelp == true)
            {
                ShowUsage();
            }
            else
            {
                if (_logFileName.Length != 0)
                {
                    _logFile = new StreamWriter(_logFileName, false);
                    _logFile.AutoFlush = true;
                }

                _startTime = DateTime.Now;

                var engine =
                    new MobiBatchConverter.Engine.Engine(_deleteInput, _rewriteOutput, _compressionLevel, _kindlegenVerbose);

                engine.ConvertStartEvent += new MobiBatchConverter.Engine.Engine.ConvertStartHandler(OnConvertStart);
                engine.ConvertCompleteEvent += new MobiBatchConverter.Engine.Engine.ConvertCompleteHandler(OnConvertComplete);

                if ((_pathArgs.Length == 2) && (Directory.Exists(_pathArgs[0]) == true))
                {
                    engine.Convert(_pathArgs[0], _pathArgs[1]);
                }
                else
                {
                    var files = new string[_pathArgs.Length - 1];

                    for (var i = 0; i < files.Length; ++i)
                    {
                        files[i] = _pathArgs[i];
                    }

                    engine.Convert(files, _pathArgs[_pathArgs.Length - 1]);
                }

                engine.ConvertCompleteEvent -= OnConvertComplete;
                engine.ConvertStartEvent -= OnConvertStart;

                if (_unconvertedFile.Count != 0)
                {
                    WriteLine("\n\nUnconverted files:");

                    for (var i = 0; i < _unconvertedFile.Count; ++i)
                    {
                        WriteFileOutput(_unconvertedFile[i], _unconvertedOutput[i], _conversionError[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Format message about conversion file task and print it to console and log file
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name="output">kindlegen console output</param>
        /// <param name="error">Error message</param>
        private void WriteFileOutput(string file, string output, string error)
        {
            var outStr = string.Empty;

            if (file.Length != 0)
            {
                outStr = file + '\n';
            }

            if (output.Length != 0)
            {
                outStr += "---------- Output stream ----------\n" + output + '\n';
            }

            if (error.Length != 00)
            {
                outStr += "---------- Error message ----------\n" + error + '\n';
            }

            if ((output.Length != 0) || (error.Length != 0))
            {
                outStr += "-----------------------------------";
            }

            if (outStr.Length != 0)
            {
                WriteLine(outStr);
            }
        }

        /// <summary>
        /// Print program usage to console
        /// </summary>
        private void ShowUsage()
        {
            Console.WriteLine("A command line multithreading e-books compiler.");
            Console.WriteLine("kindlegen.exe e-book compiler is required for running MBCCmd.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Usage: MBCCmd [-c <level>] [-d] [-r] [-v] [-l <log>] [-a] <<filename.epub/.epub.zip/.fb2/.fb2.zip> [...]|<directory>> <destination>");
            Console.WriteLine("  -c <level>: compression level for kindlegen, see kindlegen help for details");
            Console.WriteLine("  -d: delete source file if conversion success");
            Console.WriteLine("  -r: overwrite destination");
            Console.WriteLine("  -v: verbose output for kindlegen, see kindlegen help for details");
            Console.WriteLine("  -l <log>: create log file with name <log>");
            Console.WriteLine("  -a: show kindlegen output");
            Console.WriteLine("  <filename.epub/.epub.zip/.fb2/.fb2.zip>: source file, source files shall be separated by space");
            Console.WriteLine("  <directory>: source directory, directory structure will be preserved in destination");
            Console.WriteLine("  <destination>: destination directory");
        }

        /// <summary>
        /// Print program copyright to console
        /// </summary>
        private void ShowCopyright()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("MBCCmd Mobi Batch Converter v1.0 Copyright (c) 2016 Alexander Zinchenko (alexander@zinchenko.com)");
            Console.WriteLine("Project home: http://mbc.codeplex.com");
            Console.WriteLine("");
        }

        /// <summary>
        /// Write string to console and log file
        /// </summary>
        /// <param name="str">String to write</param>
        private void WriteLine(string str)
        {
            Console.WriteLine(str);

            if (_logFile != null)
            {
                _logFile.WriteLine(str);
            }
        }

        /// <summary>
        /// Parse command line parameters
        /// </summary>
        /// <returns>True if parameters are valid, false if parameters are not valid or error occurs during parsing</returns>
        private bool ParseCommandLine()
        {
            try
            {
                var i = 0;
                var pathAgrsCount = 0;
                var isOptionEnd = false;

                while (i < _args.Length)
                {
                    if (isOptionEnd == true)
                    {
                        // No more program options
                        _pathArgs[pathAgrsCount++] = _args[i++];
                    }
                    else
                    {
                        if (string.Compare(_args[i], "-h") == 0)
                        {
                            // Help option is detected
                            _showHelp = true;
                        }
                        else if (string.Compare(_args[i], "-c") == 0)
                        {
                            // Compression level option is detected
                            _compressionLevel = Convert.ToInt32(_args[i + 1]);
                            i += 2;
                        }
                        else if (string.Compare(_args[i], "-d") == 0)
                        {
                            // Delete source option is detected
                            _deleteInput = true;
                            ++i;
                        }
                        else if (string.Compare(_args[i], "-r") == 0)
                        {
                            // Rewrite output option is detected
                            _rewriteOutput = true;
                            ++i;
                        }
                        else if (string.Compare(_args[i], "-v") == 0)
                        {
                            // Verbose kindlegen output option is detected
                            _kindlegenVerbose = true;
                            ++i;
                        }
                        else if (string.Compare(_args[i], "-l") == 0)
                        {
                            // Log output to file option is detected
                            _logFileName = _args[i + 1];
                            i += 2;
                        }
                        else if (string.Compare(_args[i], "-a") == 0)
                        {
                            // Display all option is detected
                            _logAll = true;
                            ++i;
                        }
                        else if ((_args[i].Length != 0) && (_args[i][0] == '-'))
                        {
                            // Not valid option is detected
                            return false;
                        }
                        else
                        {
                            // Rest parameters are paths
                            _pathArgs = new string[_args.Length - i];
                            isOptionEnd = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return false;
            }

            if (_args.Length == 0)
            {
                _showHelp = true;
            }
            
            if ((_showHelp == false) && ((_pathArgs == null) || (_pathArgs.Length < 2)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Event handler when conversion task is started
        /// </summary>
        /// <param name="name">File name of source file</param>
        private void OnConvertStart(string name)
        {
            _fileNumsLock.EnterWriteLock();
            try
            {
                _totalFileNum++;

                var fileLength = (new FileInfo(name)).Length;

                _fileSizes.Add(name, fileLength);
                _totalFileSize += fileLength;
            }
            finally
            {
                _fileNumsLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Event handler when converion task is completed
        /// </summary>
        /// <param name="inFile">Source file</param>
        /// <param name="outFile">Destination file</param>
        /// <param name="kindlegenOutput">kindlegen output</param>
        /// <param name="conversionError">Error message if error occurs</param>
        private void OnConvertComplete(string inFile, string outFile, string kindlegenOutput, string conversionError)
        {
            ulong totalFileNum, completedFileNum;
            long totalFileSize, completedFileSize;

            _fileNumsLock.EnterWriteLock();
            try
            {
                ++_completedFileNum;
                completedFileNum = _completedFileNum;
                
                _completedFileSize += _fileSizes[inFile];
                completedFileSize = _completedFileSize;

                totalFileNum = _totalFileNum;
                totalFileSize = _totalFileSize;
                
                _fileSizes.Remove(inFile);

                if (File.Exists(outFile) == false)
                {
                    _unconvertedFile.Add(inFile);
                    _unconvertedOutput.Add(kindlegenOutput);
                    _conversionError.Add(conversionError);
                }
            }
            finally
            {
                _fileNumsLock.ExitWriteLock();
            }

            if (File.Exists(outFile) == true)
            {
                WriteLine("File #" + completedFileNum + "/" + totalFileNum + " (" + (completedFileSize * 100 / totalFileSize) +
                    "%), time left " + EstimateTimeLeft(totalFileSize, completedFileSize) + ": " + inFile);

                if (_logAll == true)
                {
                    WriteFileOutput(string.Empty, kindlegenOutput, conversionError);
                }
            }
            else
            {
                WriteLine("Failed to convert file: " + inFile);

                WriteFileOutput(string.Empty, kindlegenOutput, conversionError);
            }
        }

        /// <summary>
        /// Calculate and format estimated time left of conversion
        /// </summary>
        /// <returns>Formatted string of time left</returns>
        private string EstimateTimeLeft(long totalFileSize, long completedFileSize)
        {
            var t = DateTime.Now - _startTime;
            var timeLeft = TimeSpan.
                FromTicks((long)((decimal)t.Ticks * (decimal)totalFileSize / (decimal)completedFileSize - (decimal)t.Ticks));

            if (timeLeft.Days == 0)
            {
                return timeLeft.ToString(@"hh\:mm\:ss");
            }
            else
            {
                return timeLeft.ToString(@"d\.hh\:mm\:ss");
            }
        }
    }
}
