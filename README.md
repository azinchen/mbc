New home for Mobi Batch Converter! Moved to GitHub from https://mbc.codeplex.com
# Mobi Batch Converter
Mobi Batch Converter was designed for batch multithreading converting ePub and FictionBook fb2
e-books to Amazon Kindle supported format.
# Description
Mobi Batch Converter was designed for batch multithreading converting e-books to Amazon Kindle
supported format. Mobi Batch Converter supports ePub and FictionBook fb2 e-books and ePub and
FictionBook fb2 files archived to zip archive.
# Installation
Mobi Batch Converter does not require specific installation. Just download the latest release and
extract archive. kindlegen.exe version 2.8 or later is required for Mobi Batch converter.
Mobi Batch Converter is tested with kindlegen v.2.8.
#Program usage
Run MBCCmd.exe for showing help.

```
Usage: MBCCmd [-c <level>] [-d] [-r] [-v] [-l <log>] [-a] <<filename.epub/.epub.zip/.fb2/.fb2.zip>
              [...]|<directory>> <destination>
```

  `-c <level>` compression level for kindlegen, see kindlegen help for details
  `-d` delete source file if conversion success
  `-r` overwrite destination
  `-v` verbose output for kindlegen, see kindlegen help for details
  `-l <log>` create log file with name <log>
  `-a` show kindlegen output
  `<filename.epub/.epub.zip/.fb2/.fb2.zip>` source file, source files shall be separated by space
  `<directory>` source directory, directory structure will be preserved in destination
  `<destination>` destination directory

#Release history
v 1.0 Initial version
