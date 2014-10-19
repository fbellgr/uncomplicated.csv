using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uncomplicated.Csv
{
	public class CsvWriterSettings
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
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.Encoding'")); }
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
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.ColumnSeparator'")); }
			}
		}
		private char? m_columnSeparator = null;

		/// <summary>
		/// Determine how the line should end.
		/// Unix="\n", Windows="\r\n", OldMac="\r"
		/// </summary>
		public CsvNewLineMode NewLineMode
		{
			get { return m_newLineMode ?? CsvNewLineMode.Windows; }
			set
			{
				if (!Readonly) { m_newLineMode = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.NewLineMode'")); }
			}
		}
		private CsvNewLineMode? m_newLineMode = null;
		
		/// <summary>
		/// Character used for text qualification. There is no validation so be sure to put something printable in there.
		/// Default is double quotes.
		/// </summary>
		public char TextQualifier
		{
			get { return m_textQualifier ?? '"'; }
			set
			{
				if (!Readonly) { m_textQualifier = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.TextQualifier'")); }
			}
		}
		private char? m_textQualifier = null;

		/// <summary>
		/// Text qualilfication. AsNeeded means that it will be used if a cell contains the column separator 
		/// character, a new line character or the text qualifyer character.
		/// Default is Always.
		/// </summary>
		public CsvTextQualification TextQualification
		{
			get { return m_textQualification ?? CsvTextQualification.Always; }
			set
			{
				if (!Readonly) { m_textQualification = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvWriterSettings.TextQualification'")); }
			}
		}
		private CsvTextQualification? m_textQualification = null;
		
		public CsvWriterSettings()
		{
		}
	}

}
