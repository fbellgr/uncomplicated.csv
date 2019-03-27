using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Uncomplicated.Csv
{
    /// <summary>
    /// Csv reader
    /// </summary>
    public class CsvReader : IDisposable
    {
        private readonly object SyncRoot = new object();
        private readonly object SyncPeek = new object();

        /// <summary>
        /// Configuration
        /// </summary>
        public readonly CsvReaderSettings Settings;
        private readonly StreamReader _reader;

        /// <summary>
        /// Current encoding
        /// </summary>
        public Encoding CurrentEncoding { get { return _reader.CurrentEncoding; } }

        private bool _EOF = false;
        private bool _leaveOpen = false;
        private bool _enableQualification = false;
        private int _maxCellCount = 1; // will adjust automatically
        private int _maxRowCount = 1; // will adjust automatically

        private char _separator = ',';
        private char _qualifier = '"';
        private string _nullValue = null;

        private int _bufferOffset = 0;
        private int _actualBufferSize = 0;
        private char[] _buffer = null;

        private List<string> _currentRow = null;
        private List<bool> _currentRowQualification = null;
        private bool _peeked = false;

        /// <summary>
        /// True if the end of the file/stream has been reached
        /// </summary>
        public bool EOF { get { return _EOF && !_peeked; } }

        /// <summary>
        /// Number of rows read to the stream
        /// </summary>
        public long RowCount { get { lock (SyncRoot) { return _rowCount; } } }
        private long _rowCount = 0;

        /// <summary>
        /// Initializes a reader for a given stream and using default settings
        /// </summary>
        /// <param name="stream"></param>
        public CsvReader(Stream stream)
            : this(stream, new CsvReaderSettings(), false)
        {
        }

        /// <summary>
        /// Initializes a reader for a given stream and using default settings
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        public CsvReader(Stream stream, bool leaveOpen)
            : this(stream, new CsvReaderSettings(), leaveOpen)
        {
        }

        /// <summary>
        /// Initializes a reader for a given stream and using the specified settings
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="settings"></param>
        public CsvReader(Stream stream, CsvReaderSettings settings)
            : this(stream, settings, false)
        {
        }

        /// <summary>
        /// Initializes a reader for a given stream and using the specified settings
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="settings"></param>
        /// <param name="leaveOpen"></param>
        public CsvReader(Stream stream, CsvReaderSettings settings, bool leaveOpen)
        {
            this.Settings = settings == null ? new CsvReaderSettings() : settings.Clone();
            this._leaveOpen = leaveOpen;

            Configure();

            if (this.Settings.Encoding == null)
            {
                _reader = new StreamReader(stream, Encoding.UTF8, settings.DetectEncodingFromByteOrderMarks, Settings.ReaderBufferSize);
            }
            else
            {
                _reader = new StreamReader(stream, settings.Encoding, settings.DetectEncodingFromByteOrderMarks, Settings.ReaderBufferSize);
            }
            Settings.Readonly = true;
        }

        private void Configure()
        {
            _enableQualification = Settings.TextQualification != CsvTextQualification.None;
            _nullValue = Settings.NullValue;
            _qualifier = Settings.TextQualifier;
            _separator = Settings.ColumnSeparator;
            _buffer = new char[Settings.ParserBufferSize];
        }

        /// <summary>
        /// Reads one row in the stream.
        /// </summary>
        /// <returns></returns>
        public string[] Read()
        {
            lock (SyncRoot)
            {
                ReadRow(false);

                if (_currentRow == null)
                {
                    return null;
                }
                else
                {
                    ++_rowCount;
                    return _currentRow.ToArray();
                }
            }
        }

        /// <summary>
        /// Peeks the next row in the stream.
        /// </summary>
        /// <returns></returns>
        public string[] Peek()
        {
            lock (SyncRoot)
            {
                ReadRow(true);

                if (_currentRow == null)
                {
                    return null;
                }
                else
                {
                    return _currentRow.ToArray();
                }
            }
        }

        /// <summary>
        /// Reads one row in the stream. Does not care whether there are carriage returns or not.
        /// </summary>
        /// <returns></returns>
        private void ReadRow(bool peek)
        {
            if (_peeked)
            {
                _peeked = peek;
                return;
            }

            _peeked = peek;

            _currentRow = null;
            _currentRowQualification = null;

            if (_EOF)
            {
                return;
            }

            // The cell buffer
            // I suspect that dealing with very short cell content will be less
            // efficient this way but otherwise the performance gain is huge.
            // An possible improvement would be to offer a setting that specify that 
            // short cells are to be expected.
            var currentCell = new StringBuilder(_maxCellCount);

            char lastChar = '\0';

            bool startCell = true;
            bool qualifierOpen = false, qualifierClosed = false, qualified = false;
            bool qualifierEscaped = false;
            bool missedEOL = false;

            char currentChar = '\0';

            do
            {

                if (!missedEOL)
                {
                    // This block of code serves the purpose of retrieving
                    // the next characters in a buffered manner.

                    int charCode = -1;

                    if (_bufferOffset == _actualBufferSize)
                    {
                        // Replenish the buffer
                        _actualBufferSize = _reader.Read(_buffer, 0, _buffer.Length);
                        _bufferOffset = 0;

                        if (_actualBufferSize > 0)
                        {
                            charCode = _buffer[0];
                        }
                    }
                    else
                    {
                        // Next character available in the buffer
                        charCode = _buffer[_bufferOffset];
                    }

                    if (charCode >= 0)
                    {
                        ++_bufferOffset;
                    }

                    if (charCode < 0)
                    {
                        _EOF = true;
                        currentChar = '\0';
                    }
                    else
                    {
                        currentChar = (char)charCode;
                    }
                }
                missedEOL = false;

                // EOF or EOL are equivalent in terms of terminating a row
                bool eol = currentChar == '\n' || lastChar == '\r' || _EOF;

                // EOF, EOL or a delimiter are equivalent in terms of terminating a cell.
                bool eoc = (eol || currentChar == _separator) && !qualifierOpen;

                if (eoc)
                {
                    // End of a cell

                    int cellLength = currentCell.Length;
                    if (lastChar == '\r' && currentCell.Length > 0)
                    {
                        // Pops the last CR when terminating a line
                        // If this is merely the end of a cell, lastChar will be a delimiter and this will not happen
                        --cellLength;
                    }

                    if (cellLength > _maxCellCount)
                    {
                        // For performance reasons, we will always be using the maximum cell length
                        // as the capacity for the cell buffers of subsequent rows.
                        // The strategy will probably need some tuning and take into account abnormal
                        // and spontaneous cell count variations.
                        // For now, this seems to do nicely.
                        _maxCellCount = cellLength;
                    }

                    // Construction of the actual cell content
                    string new_cell = currentCell.ToString(0, cellLength);

                    // Reset the cell buffer.
                    // When dealing with stringbuilder, clearing seems to be more efficient
                    // than reinitializing with a new instance.
                    currentCell.Clear();

                    if (!qualifierClosed && new_cell == _nullValue)
                    {
                        new_cell = null;
                    }

                    if (_currentRow == null)
                    {
                        _currentRow = new List<string>(_maxRowCount);
                        _currentRowQualification = new List<bool>(_maxRowCount);
                    }
                    _currentRow.Add(new_cell);
                    _currentRowQualification.Add(qualified);

                    qualifierClosed = false;
                    qualifierEscaped = false;
                    qualified = false;

                    if (eol)
                    {
                        // In the event of an actual EOL or EOF,
                        // we break here.

                        if (_currentRow.Count > _maxRowCount)
                        {
                            // For performance reasons, we will always be using the maximum cell count
                            // as the capacity of subsequent rows.
                            // The strategy will probably need some tuning and take into account abnormal
                            // and spontaneous cell count variations.
                            // For now, this seems to do nicely.
                            _maxRowCount = _currentRow.Count;
                        }
                        break;
                    }

                    if (currentChar != '\n' && lastChar == '\r')
                    {
                        lastChar = '\0';
                        missedEOL = true;
                        // Double back to handle EOL. Peeking would probably more 
                        // efficient but ending lines with CR is not common.
                        continue;
                    }

                    startCell = true;
                }
                else
                {
                    // Cell content

                    bool appendCurrentChar = true;

                    if (_enableQualification)
                    {
                        // If qualification needs  to be handled

                        if (startCell && !qualifierOpen && currentChar == _qualifier)
                        {
                            // The qualifier needs to be at the begining of a cell.
                            // Otherwise the cell will be considered as not text qualified.
                            appendCurrentChar = false;
                            qualifierOpen = true;
                            qualified = true;
                        }
                        else if (qualifierOpen && currentChar == _qualifier)
                        {
                            // This is an attempt to close a text qualifier.
                            // If the next character is also a qualifier, then it will be
                            // determined that a qualifier has been escaped
                            appendCurrentChar = false;
                            qualifierOpen = false;
                            qualifierClosed = true;
                            qualifierEscaped = false;
                        }
                        else if (currentChar == _qualifier && qualifierClosed && lastChar == _qualifier && !qualifierEscaped)
                        {
                            // Detection of an escaped qualifier
                            // Text qualification is resumed and the current qualifier will be written,
                            qualifierOpen = true;
                            qualified = true;
                            qualifierClosed = false;
                            qualifierEscaped = true;
                        }
                    }

                    if (appendCurrentChar)
                    {
                        // Actual cell content
                        currentCell.Append(currentChar);
                    }

                    // This ain't the beginning of a cell no more.
                    startCell = false;
                }
                lastChar = currentChar;

            }
            // Ultimately, EOF will break the loop.
            // But it should always explicitly break in the body of the loop
            while (!_EOF);

            // ignore the last unqualified lonely cell of the file
            // this is the empty line most csv files have
            if (EOF && _currentRow?.Count == 1 && _currentRow[0].Length == 0 && !_currentRowQualification[0])
            {
                _currentRow = null;
            }
        }

        /// <summary>
        /// Closes and disposes of the underlying StreamReader and stream.
        /// </summary>
        public void Dispose()
        {
            if (_reader != null && !_leaveOpen)
            {
                _reader.Close();
                _reader.Dispose();
            }
        }
    }
}
