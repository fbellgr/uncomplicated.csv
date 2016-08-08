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
			get { return _encoding; }
			set
			{
				if (!Readonly) { _encoding = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.Encoding'")); }
			}
		}
		private Encoding _encoding = null;

		/// <summary>
		/// Value to be interpreted as null
		/// CANNOT CONTAIN SEPARATOR OR NEWLINE OR TEXT-QUALIFIER.
		/// This value must not be text-qualified in the source to be considered null.
		/// Ignored by default.
		/// </summary>
		public string NullValue
		{
			get { return _nullValue; }
			set
			{
				if (!Readonly)
				{
					_nullValue = value;
					ValidateSettings();
				}
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.NullValue'")); }
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
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.ColumnSeparator'")); }
			}
		}
		private char _columnSeparator = ',';

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
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.TextQualifier'")); }
			}
		}
		private char _textQualifier = '"';

		/// <summary>
		/// Text qualilfication. When reading, Always and AsNeeded wil produced the same result.
		/// Defauls is Always.
		/// Note that for the reader, Always and AsNeeded behave exactly the same way.
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
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.TextQualification'")); }
			}
		}
		private CsvTextQualification _textQualification = CsvTextQualification.Always;

		/// <summary>
		/// Will determine whether to use encoding detection or not.
		/// </summary>
		public bool DetectEncodingFromByteOrderMarks
		{
			get { return _detectEncodingFromByteOrderMarks; }
			set
			{
				if (!Readonly) { _detectEncodingFromByteOrderMarks = value; }
				else { throw new CsvException(string.Concat("Read only property 'CsvReaderSettings.DetectEncodingFromByteOrderMarks'")); }
			}
		}
		private bool _detectEncodingFromByteOrderMarks = false;

		public CsvReaderSettings()
		{
		}

		internal CsvReaderSettings Clone()
		{
			var clone = MemberwiseClone() as CsvReaderSettings;
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