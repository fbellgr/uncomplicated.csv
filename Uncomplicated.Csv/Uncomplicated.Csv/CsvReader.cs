using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
/* UDE
 * To enable more advanced encoding detection, add a reference to the appropriate librarie and
 * decomment the following code as well any other part of the class marked 'UDE'.
 *
 * Ude is a C# port of Mozilla Universal Charset Detector.
 * https://code.google.com/p/ude/ 
*/
//using Ude;
using System.Data;

namespace Uncomplicated.Csv
{
	/// <summary>
	/// Class for parsing a csv flat file. Does not care about the type of end of line.
	/// </summary>
	public class CsvReader : IDisposable
	{
		/// <summary>
		/// Configuration
		/// </summary>
		public readonly CsvReaderSettings Settings;

		private readonly StreamReader Reader;

		public CsvReader(Stream stream)
			: this(stream, new CsvReaderSettings())
		{
		}

		public CsvReader(Stream stream, CsvReaderSettings settings)
		{
			this.Settings = settings == null ? new CsvReaderSettings() : settings.Clone();

			#region UDE
			/* UDE */
			/*
			if (stream.CanSeek && settings.DetectEncodingFromByteOrderMarks)
			{
				try
				{
					string charset = DetectCharset(stream);

					if (!string.IsNullOrWhiteSpace(charset))
					{
						settings.Encoding = Encoding.GetEncoding(charset);
					}
				}
				catch (Exception e)
				{
				}
			}
			*/
			#endregion

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
		}

		/// <summary>
		/// Reads one row in the stream. Does not care whether there are carriage returns or not.
		/// </summary>
		/// <returns></returns>
		public string[] Read()
		{
			if (Reader.Peek() < 0)
			{
				return null;
			}

			List<string> columns = null;
			Stack<string> stack = new Stack<string>();
			char c = '\0';
			bool qualifierStart = false;
			bool qualifierEnd = false;
			bool newCell = true;

			string escapedQualifier = string.Concat(Settings.TextQualifier, Settings.TextQualifier);

			// common operations anonymous helper method
			// for a better readability of the algorithm

			Action push = () => stack.Push(c.ToString());
			Action<string> pushStr = (pushed) => stack.Push(pushed);
			Action escapeQualifier = () => stack.Push(escapedQualifier);
			Func<bool> isQualifier = () => c == Settings.TextQualifier;
			Func<bool> isSeparator = () => c == Settings.ColumnSeparator;
			Func<bool> peekQualifier = () => stack.Count > 0 && stack.Peek() == Settings.TextQualifier.ToString();
			Func<string> peek = () => stack.Count > 0 ? stack.Peek() : null;
			Func<string> pop = () => stack.Count > 0 ? stack.Pop() : null;
			Func<bool> peekCR = () => stack.Count > 0 && stack.Peek() == "\r";
			Func<bool> peekEOL = () => stack.Count > 0 && stack.Peek() == "\n";
			Func<bool> peekReaderEOL = () => Reader.Peek() > 0 && (char)Reader.Peek() == '\n';
			Func<bool> eol = () => c == '\n';
			Func<bool> cr = () => c == '\r';

			Action addCell = () =>
			{
				StringBuilder sbuf = new StringBuilder();
				while (stack.Count > 0)
				{
					string txt = pop();
					if (txt == escapedQualifier)
					{
						txt = Settings.TextQualifier.ToString();
					}
					else if (txt == Settings.TextQualifier.ToString() && qualifierStart && !qualifierEnd)
					{
						txt = string.Empty;
					}
					if (!string.IsNullOrEmpty(txt))
					{
						sbuf.Insert(0, txt);
					}
				}
				if (columns == null)
				{
					columns = new List<string>();
				}
				columns.Add(sbuf.ToString());
				newCell = true;
				qualifierEnd = qualifierStart = false;
			};

			// Line parsing
			while (Reader.Peek() >= 0)
			{
				c = (char)Reader.Read();

				if (newCell)
				{
					// starting new cell

					newCell = false;

					if (eol())
					{
						// empty row
						break;
					}
					if (cr())
					{
						// empty row
						if (peekReaderEOL())
						{
							//discard
							Reader.Read();
						}
						pushStr(string.Empty);
						break;
					}

					if (isQualifier())
					{
						// text qualified cell
						qualifierStart = true;
					}
					else if (!isSeparator())
					{
						// first character
						push();
					}
					else
					{
						// empty cell
						addCell();
					}
				}


				// Text qualifier
				else if (isQualifier())
				{
					// process text qualifier

					if (qualifierStart && peekQualifier())
					{
						// needs escaping
						// escaped quotes will be resolved when the cell is assembled
						pop();
						escapeQualifier();
					}
					else
					{
						// add qualifier on the stack
						push();
					}
				}

				// Other characters
				else
				{
					// process regular characters

					if (peekQualifier() && qualifierStart && !qualifierEnd)
					{
						// last qualifier is the closing qualifier
						pop();
						pushStr(string.Empty);
						qualifierEnd = true;
					}

					// old mac or windows EOL
					if (cr() && (!qualifierStart || qualifierEnd))
					{
						if (peekReaderEOL())
						{
							//discard
							Reader.Read();
						}
						break;
					}

					// unix EOL
					if (eol() && (!qualifierStart || qualifierEnd))
					{
						// end of row
						break;
					}

					if (isSeparator() && (!qualifierStart || qualifierEnd))
					{
						// end of cell
						addCell();
					}
					else
					{
						// add character on the stack
						push();
					}
				}

			}

			// left over cell
			if (stack.Count > 0 || isSeparator())
			{
				// last cell
				addCell();
			}

			return columns == null ? null : columns.ToArray();
		}

		/// <summary>
		/// Skips a line
		/// </summary>
		/// <returns></returns>
		public string Skip()
		{
			StringBuilder line = null;
			char last = '\0';
			bool cr = false;
			bool nl = false;
			if (Reader.Peek() >= 0)
			{
				line = new StringBuilder();
			}

			while (Reader.Peek() >= 0)
			{

				char c = (char)Reader.Peek();
				if (c == '\r')
				{
					Reader.Read();
					cr = true;
				}
				else if (c == '\n')
				{
					Reader.Read();
					nl = true;
					break;
				}
				else
				{
					if (last != '\r')
					{
						Reader.Read();
						line.Append(c);
					}
					else
					{
						break;
					}
				}

				last = c;
			}

			return line == null ? null : line.ToString();
		}

		public void Dispose()
		{
			if (Reader != null)
			{
				Reader.Close();
				Reader.Dispose();
			}
		}

		#region UDE
		/* UDE */
		/*
		/// <summary>
		/// Attempts to detect encoding.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static string DetectCharset(Stream stream)
		{
			ICharsetDetector cdet = new CharsetDetector();
			cdet.Feed(stream);
			cdet.DataEnd();
			if (stream.CanSeek)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}
			return cdet.Charset;
		}
		*/
		#endregion
	}
}
