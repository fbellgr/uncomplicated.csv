using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	/// <summary>
	/// Csv writer settings
	/// </summary>
	public class CsvWriterSettings
	{
		internal bool Readonly { get; set; }

		internal const string CR = "\r";
		internal const string CRLF = "\r\n";
		internal const string LF = "\n";

		/// <summary>
		/// Encoding of the file.
		/// </summary>
		public Encoding Encoding
		{
			get { return _encoding; }
			set
			{
				if (!Readonly) { _encoding = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.Encoding'")); }
			}
		}
		private Encoding _encoding = null;

		/// <summary>
		/// Value to be rendered in place of null. 
		/// CANNOT CONTAIN SEPARATOR OR NEWLINE OR TEXT-QUALIFIER.
		/// Default is empty.
		/// </summary>
		public string NullValue
		{
			get { return _nullValue; }
			set
			{
				if (!Readonly)
				{
					_nullValue = value ?? string.Empty;
					ValidateSettings();
				}
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.NullValue'")); }
			}
		}
		private string _nullValue = null;

		/// <summary>
		/// Character used as column separator. There is no validation so be sure to put something printable in there.
		/// Default is comma.
		/// </summary>
		public char ColumnSeparator
		{
			get { return _columnSeparator; }
			set
			{
				if (!Readonly)
				{
					_columnSeparator = value;
					ValidateSettings();
				}
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.ColumnSeparator'")); }
			}
		}
		private char _columnSeparator = ',';

		/// <summary>
		/// Determine how the line should end.
		/// Unix="\n", Windows="\r\n", OldMac="\r"
		/// </summary>
		public CsvNewLineMode NewLineMode
		{
			get { return _newLineMode; }
			set
			{
				if (!Readonly) { _newLineMode = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.NewLineMode'")); }
			}
		}
		private CsvNewLineMode _newLineMode = CsvNewLineMode.Windows;

		/// <summary>
		/// Character used for text qualification. There is no validation so be sure to put something printable in there.
		/// Default is double quotes.
		/// </summary>
		public char TextQualifier
		{
			get { return _textQualifier; }
			set
			{
				if (!Readonly)
				{
					_textQualifier = value;
					ValidateSettings();
				}
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.TextQualifier'")); }
			}
		}
		private char _textQualifier = '"';

		/// <summary>
		/// Text qualilfication. AsNeeded means that it will be used if a cell contains the column separator 
		/// character, a new line character or the text qualifyer character.
		/// Default is Always.
		/// </summary>
		public CsvTextQualification TextQualification
		{
			get { return _textQualification; }
			set
			{
				if (!Readonly)
				{
					_textQualification = value;
					ValidateSettings();
				}
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.TextQualification'")); }
			}
		}
		private CsvTextQualification _textQualification = CsvTextQualification.Always;

		//public CsvWriterSettings()
		//{
		//}

		/// <summary>
		/// Qualifies
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public string TextQualify(string cell)
		{
			var sbuf = new StringBuilder(cell.Length + 2);
			sbuf.Append(TextQualifier);
			for (int i = 0; i < cell.Length; ++i)
			{
				var c = cell[i];
				if (c == TextQualifier)
				{
					sbuf.Append(TextQualifier).Append(TextQualifier);
				}
				else
				{
					sbuf.Append(c);
				}
			}
			sbuf.Append(TextQualifier);
			return sbuf.ToString();
		}

		/// <summary>
		/// Escapes and qualifies
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public string ConvertCell(string cell)
		{
			if (cell == null)
			{
				return NullValue;
			}

			switch (TextQualification)
			{
				case CsvTextQualification.AsNeeded:
					if (
						cell.Contains(ColumnSeparator)
						|| cell.Contains('\r')
						|| cell.Contains('\n')
						|| cell.Contains(TextQualifier))
					{
						cell = TextQualify(cell);
					}
					break;

				case CsvTextQualification.Always:
					cell = TextQualify(cell);
					break;
			}

			return cell;
		}

		/// <summary>
		/// Obtains the appropriate end of line string.
		/// </summary>
		/// <returns></returns>
		public string GetEOL()
		{
			switch (NewLineMode)
			{
				case CsvNewLineMode.OldMac:
					return CR;

				default:
				case CsvNewLineMode.Windows:
					return CRLF;

				case CsvNewLineMode.Unix:
					return LF;
			}
		}

		/// <summary>
		/// Produces the data to be written for a single row.
		/// </summary>
		/// <param name="cells"></param>
		/// <returns></returns>
		public string CreateRow(params string[] cells)
		{

			if (cells == null)
			{
				throw new ArgumentNullException("cells");
			}

			string row = string.Empty;

			int count = cells.Length;
			if (count > 2)
			{
				var sbuf = new StringBuilder(count);
				for (int i = 0; i < count; ++i)
				{
					if (i > 0)
					{
						sbuf.Append(ColumnSeparator);
					}
					sbuf.Append(ConvertCell(cells[i]));
				}
				row = sbuf.ToString();
			}
			else
			{

				row = string.Join(
					new string(ColumnSeparator, 1),
					Array.ConvertAll(cells, cell => ConvertCell(cell))
				);
			}

			return row;
		}

		/// <summary>
		/// Produces the data to be written for a single row.
		/// Most efficiently handled collections are string[] and List&lt;string&gt;
		/// </summary>
		/// <param name="cells"></param>
		/// <returns></returns>
		public string CreateRow(IEnumerable<string> cells)
		{
			string row = string.Empty;

			if (cells == null)
			{
				throw new ArgumentNullException("cells");
			}

			if (cells is IList<string>)
			{
				// optimized for IList<string>
				var items = cells as IList<string>;
				int count = items is string[]
					? ((string[])items).Length
					: items.Count;

				if (count > 2)
				{
					var sbuf = new StringBuilder(count);
					for (int i = 0; i < count; ++i)
					{
						if (i > 0)
						{
							sbuf.Append(ColumnSeparator);
						}
						sbuf.Append(ConvertCell(items[i]));
					}
				}
				else
				{
					row = string.Join(new string(ColumnSeparator, 1),
						items is string[]
							? Array.ConvertAll((string[])items, cell => ConvertCell(cell))
							: (
								items is List<string>
									? ((List<string>)items).ConvertAll(cell => ConvertCell(cell))
									: items.Select(cell => ConvertCell(cell))
							)
					);
				}
			}
			else
			{
				row = string.Join(new string(ColumnSeparator, 1), cells.Select(cell => ConvertCell(cell)));
			}

			return row;
		}

		internal CsvWriterSettings Clone()
		{
			var clone = MemberwiseClone() as CsvWriterSettings;
			clone.Readonly = false;
			clone.Encoding = Encoding == null ? null : Encoding.Clone() as Encoding;
			return clone;
		}

		private void ValidateSettings()
		{
			CsvUtil.ValidateCsvSettings(NullValue, ColumnSeparator, TextQualification == CsvTextQualification.None ? '\0' : TextQualifier);
		}
	}

}