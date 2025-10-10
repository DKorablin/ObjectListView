/*
 * OLVExporter - Export the contents of an ObjectListView into various text-based formats
 *
 * Author: Phillip Piper
 * Date: 7 August 2012, 10:35pm
 *
 * Change log:
 * 2012-08-07  JPP  Initial code
 * 
 * Copyright (C) 2012 Phillip Piper
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip.piper@gmail.com.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BrightIdeasSoftware
{
	/// <summary>An OLVExporter converts a collection of rows from an ObjectListView into a variety of textual formats.</summary>
	public class OLVExporter
	{

		/// <summary>What format will be used for exporting</summary>
		public enum ExportFormat
		{

			/// <summary>Tab separated values, according to http://www.iana.org/assignments/media-types/text/tab-separated-values</summary>
			TabSeparated = 1,

			/// <summary>Alias for TabSeparated</summary>
			TSV = 1,

			/// <summary>Comma separated values, according to http://www.ietf.org/rfc/rfc4180.txt</summary>
			CSV,

			/// <summary>HTML table, according to me</summary>
			HTML
		}

		#region Life and death

		/// <summary>Create an empty exporter</summary>
		public OLVExporter() { }

		/// <summary>Create an exporter that will export all the rows of the given ObjectListView</summary>
		/// <param name="olv"></param>
		public OLVExporter(ObjectListView olv) : this(olv, olv.Objects) { }

		/// <summary>Create an exporter that will export all the given rows from the given ObjectListView</summary>
		/// <param name="olv"></param>
		/// <param name="objectsToExport"></param>
		public OLVExporter(ObjectListView olv, IEnumerable objectsToExport)
		{
			_ = objectsToExport ?? throw new ArgumentNullException(nameof(objectsToExport));

			this.ListView = olv ?? throw new ArgumentNullException(nameof(olv));
			this.ModelObjects = ObjectListView.EnumerableToArray(objectsToExport, true);
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets whether hidden columns will also be included in the textual representation.</summary>
		/// <remarks>If this is false (the default), only visible columns will be included.</remarks>
		public Boolean IncludeHiddenColumns { get; set; }

		/// <summary>Gets or sets whether column headers will also be included in the text and HTML representation.</summary>
		/// <remarks>Default is true.</remarks>
		public Boolean IncludeColumnHeaders { get; set; } = true;

		/// <summary>Gets the ObjectListView that is being used as the source of the data to be exported</summary>
		public ObjectListView ListView { get; set; }

		/// <summary>Gets the model objects that are to be placed in the data Object</summary>
		public IList ModelObjects { get; set; } = new ArrayList();

		#endregion

		#region Commands

		/// <summary>Export the nominated rows from the nominated ObjectListView.</summary>
		/// <param name="format"></param>
		/// <returns>Returns the result in the expected format.</returns>
		/// <remarks>This will perform only one conversion, even if called multiple times with different formats.</remarks>
		public String ExportTo(ExportFormat format)
		{
			if(this._results == null)
				this.Convert();

			return this._results[format];
		}

		/// <summary>Convert </summary>
		public void Convert()
		{

			IList<OLVColumn> columns = this.IncludeHiddenColumns ? this.ListView.AllColumns : this.ListView.ColumnsInDisplayOrder;

			StringBuilder sbText = new StringBuilder();
			StringBuilder sbCsv = new StringBuilder();
			StringBuilder sbHtml = new StringBuilder("<table>");

			// Include column headers
			if(this.IncludeColumnHeaders)
			{
				List<String> strings = new List<String>();
				foreach(OLVColumn col in columns)
					strings.Add(col.Text);

				WriteOneRow(sbText, strings, "", "\t", "", null);
				WriteOneRow(sbHtml, strings, "<tr><td>", "</td><td>", "</td></tr>", HtmlEncode);
				WriteOneRow(sbCsv, strings, "", ",", "", CsvEncode);
			}

			foreach(Object modelObject in this.ModelObjects)
			{
				List<String> strings = new List<String>();
				foreach(OLVColumn col in columns)
					strings.Add(col.GetStringValue(modelObject));

				WriteOneRow(sbText, strings, "", "\t", "", null);
				WriteOneRow(sbHtml, strings, "<tr><td>", "</td><td>", "</td></tr>", HtmlEncode);
				WriteOneRow(sbCsv, strings, "", ",", "", CsvEncode);
			}
			sbHtml.AppendLine("</table>");

			_results = new Dictionary<ExportFormat, String>
			{
				[ExportFormat.TabSeparated] = sbText.ToString(),
				[ExportFormat.CSV] = sbCsv.ToString(),
				[ExportFormat.HTML] = sbHtml.ToString()
			};

			void WriteOneRow(StringBuilder sb, IEnumerable<String> strings, String startRow, String betweenCells, String endRow, StringToString encoder)
			{
				sb.Append(startRow);
				Boolean first = true;
				foreach(String s in strings)
				{
					if(!first)
						sb.Append(betweenCells);
					sb.Append(encoder == null ? s : encoder(s));
					first = false;
				}
				sb.AppendLine(endRow);
			}
		}

		private delegate String StringToString(String str);

		private Dictionary<ExportFormat, String> _results;

		#endregion

		#region Encoding

		/// <summary>
		/// Encode a String such that it can be used as a value in a CSV file.
		/// This basically means replacing any quote mark with two quote marks,
		/// and enclosing the whole String in quotes.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private static String CsvEncode(String text)
		{
			if(text == null)
				return null;

			const String DOUBLEQUOTE = @""""; // one double quote
			const String TWODOUBEQUOTES = @""""""; // two double quotes

			StringBuilder sb = new StringBuilder(DOUBLEQUOTE);
			sb.Append(text.Replace(DOUBLEQUOTE, TWODOUBEQUOTES));
			sb.Append(DOUBLEQUOTE);

			return sb.ToString();
		}

		/// <summary>HTML-encodes a String and returns the encoded String.</summary>
		/// <param name="text">The text String to encode.</param>
		/// <returns>The HTML-encoded text.</returns>
		/// <remarks>Taken from http://www.west-wind.com/weblog/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb</remarks>
		private static String HtmlEncode(String text)
		{
			if(text == null)
				return null;

			StringBuilder sb = new StringBuilder(text.Length);

			Int32 len = text.Length;
			for(Int32 i = 0; i < len; i++)
			{
				switch(text[i])
				{
				case '<':
					sb.Append("&lt;");
					break;
				case '>':
					sb.Append("&gt;");
					break;
				case '"':
					sb.Append("&quot;");
					break;
				case '&':
					sb.Append("&amp;");
					break;
				default:
					if(text[i] > 159)
					{
						// decimal numeric entity
						sb.Append("&#");
						sb.Append(((Int32)text[i]).ToString(CultureInfo.InvariantCulture));
						sb.Append(";");
					} else
						sb.Append(text[i]);
					break;
				}
			}
			return sb.ToString();
		}
		#endregion
	}
}