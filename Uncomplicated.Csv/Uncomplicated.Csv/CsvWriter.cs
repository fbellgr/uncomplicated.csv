using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Uncomplicated.Csv
{
	/// <summary>
	/// Csv writer
	/// </summary>
	public class CsvWriter : IDisposable
	{
		private readonly object SyncRoot = new object();

		/// <summary>
		/// Configuration
		/// </summary>
		public readonly CsvWriterSettings Settings;

		private readonly StreamWriter Writer;

		/// <summary>
		/// Initializesa CsvWriter for a given stream and using the default settings
		/// </summary>
		/// <param name="stream"></param>
		public CsvWriter(Stream stream)
			: this(stream, new CsvWriterSettings())
		{
		}

		/// <summary>
		/// Initializes a CsvWriter for a given stream and using the specified settings
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="settings"></param>
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
			string row = Settings.CreateRow(cells);
			Write(row);
		}

		/// <summary>
		/// Writes a row
		/// Most efficiently handled collections are string[] and List&lt;string&gt;
		/// </summary>
		/// <param name="cells">Cells to be written</param>
		public void WriteRow(IEnumerable<string> cells)
		{
			string row = Settings.CreateRow(cells);
			Write(row);
		}

		private void Write(string row)
		{
			lock (SyncRoot)
			{
				Writer.Write(row);
				Writer.Write(Settings.GetEOL());
			}
		}

		/// <summary>
		/// Flushes, closes and disposes of the underlying StreamWriter and stream.
		/// </summary>
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
