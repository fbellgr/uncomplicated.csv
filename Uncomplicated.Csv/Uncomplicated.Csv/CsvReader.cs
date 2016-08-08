using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Uncomplicated.Csv.Collection;

namespace Uncomplicated.Csv
{
	/// <summary>
	/// Class for parsing a csv flat file. Does not care about the type of end of line.
	/// </summary>
	public class CsvReader : IDisposable
	{
		private readonly object SyncRoot = new object();

		/// <summary>
		/// Configuration
		/// </summary>
		public readonly CsvReaderSettings Settings;
		private readonly StreamReader Reader;

		string _escapedQualifier = null;
		string _qualiferString = null;
		bool _enableQualification = false;

		private int _bufferOffset = 0;
		private int _bufferSize = 0;
		private char[] _buffer = new char[4096];// 4k buffer

		public CsvReader(Stream stream)
			: this(stream, new CsvReaderSettings())
		{
		}

		public CsvReader(Stream stream, CsvReaderSettings settings)
		{
			this.Settings = settings == null ? new CsvReaderSettings() : settings.Clone();

			if (this.Settings.Encoding == null)
			{
				Reader = new StreamReader(stream, settings.DetectEncodingFromByteOrderMarks);
			}
			else
			{
				Reader = new StreamReader(stream, settings.Encoding, settings.DetectEncodingFromByteOrderMarks);
			}
			Settings.Encoding = Reader.CurrentEncoding;
			Settings.Readonly = true;

			_enableQualification = Settings.TextQualification != CsvTextQualification.None;
			_escapedQualifier = new string(Settings.TextQualifier, 2);
			_qualiferString = new string(Settings.TextQualifier, 1);
		}

		private void PushEscapedQualifier(FakeStack stack)
		{
			stack.Push(_escapedQualifier);
		}

		private bool IsQualifierStarted(bool qualifierStart, bool qualifierEnd)
		{
			return _enableQualification && qualifierStart && !qualifierEnd;
		}

		private bool IsQualifierEnded(bool qualifierStart, bool qualifierEnd)
		{
			return _enableQualification && qualifierStart && qualifierEnd;
		}

		private bool IsQualifier(char c)
		{
			return _enableQualification && c.Equals(Settings.TextQualifier);
		}

		private bool IsSeparator(char c)
		{
			return c == Settings.ColumnSeparator;
		}

		private bool PeekQualifier(FakeStack stack)
		{
			return stack.Count > 0 && _enableQualification && stack.Peek(Settings.TextQualifier);
		}

		private bool PeekNewLine()
		{
			return PeekChar().Equals((int)'\n');
		}

		private bool IsNewLine(char c)
		{
			return c.Equals('\n');
		}

		private bool IsCarriageReturn(char c)
		{
			return c.Equals('\r');
		}

		private void AddCell(FakeStack stack, ref List<string> columns, ref bool newCell, ref bool qualifierStart, ref bool qualifierEnd)
		{
			var parts = new List<string>();
			while (stack.Count > 0)
			{
				string txt = stack.Pop();
				if (txt != null)
				{
					if (_enableQualification)
					{
						if (txt.Length == 2 && txt[0].Equals(Settings.TextQualifier) && txt[1].Equals(Settings.TextQualifier))
						{
							txt = _qualiferString;
							parts.Add(txt);
						}
						else if (txt.Length != 1 || !txt[0].Equals(Settings.TextQualifier) || IsQualifierStarted(qualifierStart, qualifierEnd))
						{
							parts.Add(txt);
						}
					}
					else
					{
						parts.Add(txt);
					}
				}
			}

			if (columns == null)
			{
				columns = new List<string>();
			}

			string val = string.Empty;

			if (parts.Count > 2)
			{
				var sbuf = new StringBuilder();
				for (int i = parts.Count - 1; i >= 0; --i)
				{
					sbuf.Append(parts[i]);
				}
				val = sbuf.ToString();
			}
			else
			{
				val = string.Concat(parts.Reverse<string>());
			}

			if (val.Equals(Settings.NullValue) && !IsQualifierEnded(qualifierStart, qualifierEnd))
			{
				val = null;
			}

			columns.Add(val);
			newCell = true;
			qualifierEnd = qualifierStart = false;
		}

		/// <summary>
		/// Reads one row in the stream. Does not care whether there are carriage returns or not.
		/// </summary>
		/// <returns></returns>
		public string[] Read()
		{
			lock (SyncRoot)
			{
				return ReadRow();
			}
		}

		/// <summary>
		/// Reads one row in the stream. Does not care whether there are carriage returns or not.
		/// </summary>
		/// <returns></returns>
		private string[] ReadRow()
		{
			if (PeekChar() < 0)
			{
				return null;
			}

			List<string> columns = null;
			var stack = new FakeStack();
			char c = '\0';
			bool qualifierStart = false;
			bool qualifierEnd = false;
			bool newCell = true;

			// common operations anonymous helper method
			// for a better readability of the algorithm

			// Line parsing
			while (PeekChar() >= 0)
			{
				c = (char)ReadChar();

				if (newCell)
				{
					// starting new cell

					newCell = false;

					if (IsNewLine(c))
					{
						// empty row
						stack.Push(string.Empty);
						break;
					}

					else if (IsCarriageReturn(c))
					{
						// empty row
						if (PeekNewLine())
						{
							//discard
							ReadChar();
						}
						stack.Push(string.Empty);
						break;
					}

					else if (IsQualifier(c))
					{
						// text qualified cell
						qualifierStart = true;
					}

					else if (!IsSeparator(c))
					{
						// first character
						stack.Push(c);
					}

					else
					{
						// empty cell
						AddCell(stack, ref columns, ref newCell, ref qualifierStart, ref qualifierEnd);
					}
				}


				// Text qualifier
				else if (IsQualifier(c))
				{
					// process text qualifier

					if (PeekQualifier(stack) && IsQualifierStarted(qualifierStart, qualifierEnd))
					{
						// needs escaping
						// escaped quotes will be resolved when the cell is assembled
						stack.Pop();
						PushEscapedQualifier(stack);
					}
					else
					{
						// add qualifier on the stack
						stack.Push(c);
					}
				}

				// Other characters
				else
				{
					// process regular characters

					if (PeekQualifier(stack) && IsQualifierStarted(qualifierStart, qualifierEnd))
					{
						// last qualifier is the closing qualifier
						stack.Pop();
						stack.Push(string.Empty);
						qualifierEnd = true;
					}

					// old mac or windows EOL
					if (IsCarriageReturn(c) && !IsQualifierStarted(qualifierStart, qualifierEnd))
					{
						if (PeekNewLine())
						{
							//discard
							ReadChar();
						}
						break;
					}

					// unix EOL
					else if (IsNewLine(c) && !IsQualifierStarted(qualifierStart, qualifierEnd))
					{
						// end of row
						break;
					}

					else if (IsSeparator(c) && !IsQualifierStarted(qualifierStart, qualifierEnd))
					{
						// end of cell
						AddCell(stack, ref columns, ref newCell, ref qualifierStart, ref qualifierEnd);
					}

					else
					{
						// add character on the stack
						stack.Push(c);
					}
				}

			}

			// left over cell
			if (stack.Count > 0 || IsSeparator(c))
			{
				// last cell
				AddCell(stack, ref columns, ref newCell, ref qualifierStart, ref qualifierEnd);
			}

			string[] arr = null;
			if (columns.Count > 0)
			{
				arr = new string[columns.Count];
				columns.CopyTo(arr);
			}

			return arr;
			//return columns == null ? null : columns.ToArray();
		}

		private int PeekChar()
		{
			int c = -1;

			if (_bufferOffset == _bufferSize)
			{
				_bufferSize = Reader.Read(_buffer, 0, _buffer.Length);
				_bufferOffset = 0;
				if (_bufferSize > 0)
				{
					c = _buffer[0];
				}
			}
			else
			{
				c = _buffer[_bufferOffset];
			}

			return c;
		}

		private int ReadChar()
		{
			int c = PeekChar();
			if (c >= 0)
			{
				++_bufferOffset;
			}
			return c;
		}

		public void Dispose()
		{
			if (Reader != null)
			{
				Reader.Close();
				Reader.Dispose();
			}
		}
	}
}
