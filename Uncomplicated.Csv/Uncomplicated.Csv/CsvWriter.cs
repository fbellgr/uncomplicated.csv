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

		/// <summary>
		/// Number of rows written to the stream
		/// </summary>
		public long RowCount { get { lock (SyncRoot) { return _rowCount; } } }
		private long _rowCount = 0;

		private readonly StreamWriter Writer;
		private bool _leaveOpen = false;


		private string _eol;
		private bool _endsWithNewLine;

		/// <summary>
		/// Initializesa CsvWriter for a given stream and using the default settings
		/// </summary>
		/// <param name="stream"></param>
		public CsvWriter(Stream stream)
			: this(stream, new CsvWriterSettings(), false)
		{
		}

		/// <summary>
		/// Initializesa CsvWriter for a given stream and using the default settings
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="leaveOpen"></param>
		public CsvWriter(Stream stream, bool leaveOpen)
			: this(stream, new CsvWriterSettings(), leaveOpen)
		{
		}

		/// <summary>
		/// Initializes a CsvWriter for a given stream and using the specified settings
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="settings"></param>
		public CsvWriter(Stream stream, CsvWriterSettings settings)
			: this(stream, settings, false)
		{
		}

		/// <summary>
		/// Initializes a CsvWriter for a given stream and using the specified settings
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="settings"></param>
		/// <param name="leaveOpen"></param>
		public CsvWriter(Stream stream, CsvWriterSettings settings, bool leaveOpen)
		{
			this.Settings = settings == null ? new CsvWriterSettings() : settings.Clone();
			this._leaveOpen = leaveOpen;

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
			Configure();
		}

		private void Configure()
		{
			_eol = Settings.GetEOL();
			_endsWithNewLine = Settings.EndsWithNewLine;
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
				if (!_endsWithNewLine && _rowCount > 0)
				{
					Writer.Write(_eol);
				}

				Writer.Write(row);
				++_rowCount;

				if (_endsWithNewLine)
				{
					Writer.Write(_eol);
				}
			}
		}

		public void Flush()
		{
			lock (SyncRoot)
			{
				Writer.Flush();
			}
		}

		/// <summary>
		/// Flushes, closes and disposes of the underlying StreamWriter and stream.
		/// </summary>
		public void Dispose()
		{
			if (Writer != null)
			{
				Writer.Flush();
				if (!_leaveOpen)
				{
					Writer.Close();
					Writer.Dispose();
				}
			}
		}
	}
}
