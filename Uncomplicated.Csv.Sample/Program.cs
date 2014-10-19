using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace Uncomplicated.Csv.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			string path = @"U:\tmp\test.csv";

			if (!string.IsNullOrWhiteSpace(path))
			{
				Write(path);
				Read(path);
			}
		}

		static void Read(string path)
		{
			CsvReaderSettings settings = new CsvReaderSettings();
			settings.ColumnSeparator = ';';
			settings.Encoding = Encoding.UTF8;
			settings.TextQualification = CsvTextQualification.AsNeeded;
			settings.TextQualifier = '"';
						
			using (Stream stream = File.OpenRead(path))
			using (CsvReader reader = new CsvReader(stream, settings))
			{
				string[] row = null;
				int i=1;
				while ((row = reader.Read()) != null)
				{
					Console.WriteLine(string.Concat("Row ", i, ":"));
					Console.WriteLine(string.Join("\t", row));
					++i;
				}
			}
		}

		static void Write(string path)
		{
			CsvWriterSettings settings = new CsvWriterSettings();
			settings.ColumnSeparator = ';';
			settings.Encoding = Encoding.UTF8;
			settings.NewLineMode = CsvNewLineMode.Windows;
			settings.TextQualification = CsvTextQualification.AsNeeded;
			settings.TextQualifier = '"';

			using (Stream stream = File.Create(path))
			using (CsvWriter writer = new CsvWriter(stream, settings))
			{
				writer.WriteRow("header1", "header2", "header3");
				writer.WriteRow("row1column1", "row1column2", "row1column3");
				writer.WriteRow("row2column1", "row2column2", "row2column3");
			}
		}
	}
}
