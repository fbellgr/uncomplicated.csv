using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	public class CsvUtil
	{
		/// <summary>
		/// Extracts a CSV file to a DataTable
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="columnNamesInFirstRow"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static DataTable GetDataTable(Stream stream, int skipLines, bool columnNamesInFirstRow, CsvReaderSettings settings, out string[] skippedLines)
		{
			DataTable dt = new DataTable();

			using (CsvReader reader = new CsvReader(stream, settings))
			{
				List<string> skipped = new List<string>();
				for (int i = 0; i < skipLines;++i )
				{
					string s = reader.Skip();
					if (s != null)
					{
						skipped.Add(s);
					}
					else
					{
						break;
					}
				}

				skippedLines = skipped.ToArray();

				// column names
				if (columnNamesInFirstRow)
				{
					CreateHeaders(reader, dt);
				}

				string[] row = null;
				int ri = 0;
				while ((row = reader.Read()) != null)
				{
					if (ri == 0 && !columnNamesInFirstRow)
					{
						for (int i = 0; i < row.Length; ++i)
						{
							string name = string.Concat("F", i + 1);
							dt.Columns.Add(name, typeof(string));
						}
					}
					dt.Rows.Add(row);
					++ri;
				}
			}

			return dt;
		}

		/// <summary>
		/// Extraction of the column names from the first row
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="dt"></param>
		private static void CreateHeaders(CsvReader reader, DataTable dt)
		{
			string[] headers = reader.Read();
			if (headers != null)
			{
				int columnIndex = 1;
				foreach (string header in headers)
				{
					string name = (header ?? string.Empty).Trim();
					bool empty = name.Length == 0;
					if (empty)
					{
						// empty cell - undefined
						// automatic generation "F#'
						int i = 0;
						while (dt.Columns.Contains(name))
						{
							name = string.Concat("F", columnIndex + i);
							++i;
						}
					}
					else
					{
						int i = 1;
						if (dt.Columns.Contains(name))
						{
							string namebkp = name;
							// column name redundancy
							// redundant column names are automatically numbered (ex: col, col_1, col_2)
							while (dt.Columns.Contains(name))
							{
								name = string.Concat(namebkp, "_", i);
								++i;
							}
						}
					}

					dt.Columns.Add(name, typeof(string));

					++columnIndex;
				}
			}
		}
	}
}
