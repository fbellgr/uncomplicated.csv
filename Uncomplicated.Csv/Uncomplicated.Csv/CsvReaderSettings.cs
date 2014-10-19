using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	public class CsvReaderSettings
	{
		internal bool Readonly { get; set; }

		/// <summary>
		/// Encoding of the file.
		/// </summary>
		public Encoding Encoding
		{
			get { return m_encoding; }
			set
			{
				if (!Readonly) { m_encoding = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.Encoding'")); }
			}
		}
		private Encoding m_encoding = null;

		/// <summary>
		/// Character used as column separator. There is no validation so be sure to put something printable in there.
		/// Default is comma.
		/// </summary>
		public char ColumnSeparator
		{
			get { return m_columnSeparator ?? ','; }
			set
			{
				if (!Readonly) { m_columnSeparator = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.ColumnSeparator'")); }
			}
		}
		private char? m_columnSeparator = null;

		/// <summary>
		/// Character used for text qualification. There is no validation so be sure to put something printable in there.
		/// Default is double quotes.
		/// </summary>
		public char TextQualifier
		{
			get { return m_textQualifier ?? '"'; }
			set
			{
				if (!Readonly)  { m_textQualifier = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.TextQualifier'")); }
			}
		}
		private char? m_textQualifier = null;

		/// <summary>
		/// Text qualilfication. When reading, Always and AsNeeded wil produced the same result.
		/// Defauls is Always.
		/// </summary>
		public CsvTextQualification TextQualification
		{
			get { return m_textQualification ?? CsvTextQualification.Always; }
			set
			{
				if (!Readonly) { m_textQualification = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.TextQualification'")); }
			}
		}
		private CsvTextQualification? m_textQualification = null;

		/// <summary>
		/// Will determine whether to use encoding detection or not.
		/// </summary>
		public bool DetectEncodingFromByteOrderMarks
		{
			get { return m_detectEncodingFromByteOrderMarks ?? false; }
			set
			{
				if (!Readonly) { m_detectEncodingFromByteOrderMarks = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.DetectEncodingFromByteOrderMarks'")); }
			}
		}
		private bool? m_detectEncodingFromByteOrderMarks = null;

		public CsvReaderSettings()
		{
		}
	}
}
