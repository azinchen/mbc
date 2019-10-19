New home for Mobi Batch Converter! Moved to GitHub from https://mbc.codeplex.com

Mobi Batch Converted was designed for batch multithreading converting ePub and FictionBook fb2
e-books to Amazon Kindle supported format.

==============
1. Description
--------------
Mobi Batch Converted was designed for batch multithreading converting e-books to Amazon Kindle
supported format. Mobi Batch Converter supports ePub and FictionBook fb2 e-books and ePub and
FictionBook fb2 files archived to zip archive.

===============
1. Installation
---------------
Mobi Batch Converted does not require specific installation. Just download the latest release and
extract archive. kindlegen.exe version 2.8 or later is required for Mobi Batch converter.
Mobi Batch Converter is tested with kindlegen v.2.8.

================
2. Program usage
----------------
Run MBCCmd.exe for showing help.

Usage: MBCCmd [-c <level>] [-d] [-r] [-v] [-l <log>] [-a] <<filename.epub/.epub.zip/.fb2/.fb2.zip>
		[...]|<directory>> <destination>

  -c <level>: compression level for kindlegen, see kindlegen help for details
  -d: delete source file if conversion success
  -r: overwrite destination
  -v: verbose output for kindlegen, see kindlegen help for details
  -l <log>: create log file with name <log>
  -a: show kindlegen output
  <filename.epub/.epub.zip/.fb2/.fb2.zip>: source file, source files shall be separated by space
  <directory>: source directory, directory structure will be preserved in destination
  <destination>: destination directory

==================
3. Release history
------------------
v 1.0 Initial version

==========
4. Licence
----------
This program is licensed under the MIT Licence

The MIT License (MIT)

Copyright (c) 2016 Alexander Zinchenko (alexander@zinchenko.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
