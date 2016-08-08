using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Uncomplicated.Csv
{
	public class CsvWriter : IDisposable
	{
		private readonly object SyncRoot = new object();

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
			Settings.Encoding = Writer.Encoding;
			Settings.Readonly = true;
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
			string row = Settings.CreateRow(cells);
			lock (SyncRoot)
			{
				Writer.Write(row);
				Writer.Write(Settings.GetEOL());
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
