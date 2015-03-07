using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Uncomplicated.Csv
{
	public class CsvWriter : IDisposable
	{
		/// <summary>
		/// Configuration
		/// </summary>
		public readonly CsvWriterSettings Settings;

		private readonly StreamWriter Writer;

		public CsvWriter(Stream stream)
			: this(stream, new CsvWriterSettings())
		{
		}

		public CsvWriter(Stream stream, CsvWriterSettings settings)
		{
			this.Settings = settings == null ? new CsvWriterSettings() : settings.Clone();

			if (settings.Encoding != null)
			{
				Writer = new StreamWriter(stream, settings.Encoding);
			}
			else
			{
				Writer = new StreamWriter(stream);
			}
			settings.Encoding = Writer.Encoding;
			settings.Readonly = true;
		}

		private string TextQualify(string cell)
		{
			cell = string.Concat(
				Settings.TextQualifier,
				cell.Replace(Settings.TextQualifier.ToString(), string.Concat(Settings.TextQualifier, Settings.TextQualifier)),
				Settings.TextQualifier
			);
			return cell;
		}

		private string ConvertCell(string cell)
		{
			switch (Settings.TextQualification)
			{
				case CsvTextQualification.AsNeeded:
					if (
						cell.Contains(Settings.ColumnSeparator)
						|| cell.Contains('\r')
						|| cell.Contains('\n')
						|| cell.Contains(Settings.TextQualifier))
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
		/// Writes a row
		/// </summary>
		/// <param name="cells">Cells to be written</param>
		public void WriteRow(params string[] cells)
		{
			WriteRow(cells.ToList());
		}

		/// <summary>
		/// Writes a row
		/// </summary>
		/// <param name="cells">Cells to be written</param>
		public void WriteRow(IEnumerable<string> cells)
		{
			string row = string.Join(Settings.ColumnSeparator.ToString(), cells.Select(cell => ConvertCell(cell)));
			Writer.Write(row);
			switch (Settings.NewLineMode)
			{
				case CsvNewLineMode.OldMac:
					Writer.Write("\r");
					break;

				case CsvNewLineMode.Windows:
					Writer.Write("\r\n");
					break;

				case CsvNewLineMode.Unix:
					Writer.Write("\n");
					break;

			}
		}

		public void Dispose()
		{
			if (Writer != null)
			{
				Writer.Close();
				Writer.Dispose();
			}
		}
	}
}
