/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 10/21/2008 11:01 PM
 * 
 * CHANGE LOG:
 * when who what
 * 10/21/2008 JPP  Initial Version
 */

using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace BrightIdeasSoftware.Tests
{
	/// <summary>Class with program entry point.</summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		//[STAThread]
		//private static void Main(String[] args) {
		//    Application.EnableVisualStyles();
		//    Application.SetCompatibleTextRenderingDefault(false);
		//    Application.Run(new MainForm());
		//    MyGlobals g = new MyGlobals();
		//    g.RunBeforeAnyTests();

		//    TestMunger t = new TestMunger();
		//    t.Init();
		//    t.Test_PutValue_NonVirtualProperty_CalledOnBaseClass();

		//    g.RunAfterAnyTests();
		//}

		//[STAThread]
		//private static void Main(String[] args) {
		//    int start = Environment.TickCount;
		//    int iterations = 10000;

		//    Person person1 = new Person("name", "occupation", 100, DateTime.Now, 1.0, true, "  photo  ", "comments");
		//    Person2 person2 = new Person2("name", "occupation", 100, DateTime.Now, 1.0, true, "  photo  ", "comments");

		//    OLVColumn column1 = new OLVColumn("ignored", "CulinaryRating");
		//    OLVColumn column2 = new OLVColumn("ignored", "BirthDate.Year.ToString.Length");
		//    OLVColumn column3 = new OLVColumn("ignored", "Photo.ToString.Trim");

		//    for (int i = 0; i < iterations; i++) {
		//        column1.GetValue(person1);
		//        column1.GetValue(person2);
		//        column2.GetValue(person1);
		//        column2.GetValue(person2);
		//        column3.GetValue(person1);
		//        column3.GetValue(person2);
		//    }

		//    Console.WriteLine("Elapsed time: {0}ms", Environment.TickCount - start);
		//}

		// Base line: Elapsed time: 2040ms
		// Base line: Elapsed time: 1547ms 2010-08-04
		// Base line: Elapsed time:  442ms 2010-08-10 New SimpleMunger implementation

		[STAThread]
		private static void Main(String[] args)
		{
			GenerateDocs();
		}

		static void GenerateDocs()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("");
			sb.AppendLine("Controls");
			sb.AppendLine("--------");

			GenerateDocs(typeof(ObjectListView), sb);
			GenerateDocs(typeof(DataListView), sb);
			GenerateDocs(typeof(VirtualObjectListView), sb);
			GenerateDocs(typeof(FastObjectListView), sb);
			GenerateDocs(typeof(TreeListView), sb);

			sb.AppendLine("");
			sb.AppendLine("Renderers");
			sb.AppendLine("---------");

			GenerateDocs(typeof(IRenderer), sb);
			GenerateDocs(typeof(AbstractRenderer), sb);
			GenerateDocs(typeof(BaseRenderer), sb);
			GenerateDocs(typeof(ImageRenderer), sb);
			GenerateDocs(typeof(MultiImageRenderer), sb);
			GenerateDocs(typeof(BarRenderer), sb);

			sb.AppendLine("");
			sb.AppendLine("Decorations");
			sb.AppendLine("-----------");

			GenerateDocs(typeof(IDecoration), sb);
			GenerateDocs(typeof(AbstractDecoration), sb);
			GenerateDocs(typeof(ImageDecoration), sb);
			GenerateDocs(typeof(TextDecoration), sb);
			GenerateDocs(typeof(RowBorderDecoration), sb);
			GenerateDocs(typeof(CellBorderDecoration), sb);
			GenerateDocs(typeof(LightBoxDecoration), sb);
			GenerateDocs(typeof(TintedColumnDecoration), sb);

			sb.AppendLine("");
			sb.AppendLine("Drag and Drop");
			sb.AppendLine("-------------");

			GenerateDocs(typeof(IDropSink), sb);
			GenerateDocs(typeof(AbstractDropSink), sb);
			GenerateDocs(typeof(SimpleDropSink), sb);
			GenerateDocs(typeof(RearrangingDropSink), sb);

			GenerateDocs(typeof(IDragSource), sb);
			GenerateDocs(typeof(AbstractDragSource), sb);
			GenerateDocs(typeof(SimpleDragSource), sb);
			GenerateDocs(typeof(OLVDataObject), sb);

			Clipboard.Clear();
			Clipboard.SetText(sb.ToString());
		}

		static Boolean IsStatic(PropertyInfo prop)
			=> prop.GetGetMethod()?.IsStatic ?? false;

		static void GenerateDocs(Type type, StringBuilder sb)
		{

			sb.AppendLine("");
			sb.AppendLine(type.Name);
			sb.AppendLine(new String('~', type.Name.Length));

			GeneratePropertiesDocs(type, sb);
			GenerateEventsDocs(type, sb);
			GenerateMethodsDocs(type, sb);
		}

		static void GeneratePropertiesDocs(Type type, StringBuilder sb)
		{

			List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties(
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly
			));
			if(properties.Count == 0)
				return;

			properties.Sort(delegate (PropertyInfo x, PropertyInfo y)
			{
				if(IsStatic(x) == IsStatic(y))
				{
					return x.Name.CompareTo(y.Name);
				} else
				{
					return IsStatic(x) ? -1 : 1;
				}
			});

			sb.AppendLine("");
			sb.AppendLine("Properties");
			sb.AppendLine("^^^^^^^^^^");

			List<String[]> values = new List<String[]>();
			values.Add(new String[] { "*Name*", "*Info*", "*Description*" });

			foreach(PropertyInfo pinfo in properties)
			{

				values.Add(new String[] { }); // marks a new entry
				DescriptionAttribute descAttr = Attribute.GetCustomAttribute(pinfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
				//OLVDocAttribute docAttr = Attribute.GetCustomAttribute(pinfo, typeof(OLVDocAttribute)) as OLVDocAttribute;
				values.Add(new String[] {
						GetName(pinfo),
						String.Format("* Type: {0}",  GetTypeName(pinfo.PropertyType)),
						(descAttr == null ? "" : descAttr.Description)
					});
				DefaultValueAttribute defaultValueAttr = Attribute.GetCustomAttribute(pinfo, typeof(DefaultValueAttribute)) as DefaultValueAttribute;
				if(defaultValueAttr != null)
					values.Add(new String[] {
							String.Empty,
							String.Format("* Default: {0}",  Convert.ToString(defaultValueAttr.Value)),
							String.Empty
						});
				CategoryAttribute categoryAttr = Attribute.GetCustomAttribute(pinfo, typeof(CategoryAttribute)) as CategoryAttribute;
				values.Add(new String[] {
						String.Empty,
						String.Format("* IDE?: {0}",  (categoryAttr == null || categoryAttr.Category != "ObjectListView" ? "No" : "Yes")),
						String.Empty
					});
				values.Add(new String[] {
						String.Empty,
						String.Format("* Access: {0}",  GetAccessLevel(pinfo)),
						String.Empty
					});
				values.Add(new String[] {
						String.Empty,
						String.Format("* Writeable: {0}",  (pinfo.CanWrite ? "Read-Write" : "Read")),
						String.Empty
					});
			}
			GenerateTable(sb, values);

		}


		static void GenerateEventsDocs(Type type, StringBuilder sb)
		{
			List<EventInfo> events = new List<EventInfo>(type.GetEvents(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
			));
			if(events.Count == 0)
				return;

			events.Sort(delegate (EventInfo x, EventInfo y)
			{
				return x.Name.CompareTo(y.Name);
			});

			sb.AppendLine("");
			sb.AppendLine("Events");
			sb.AppendLine("^^^^^^");

			List<String[]> values = new List<String[]>();
			values.Add(new String[] { "*Name*", "*Parameters*", "*Description*" });
			foreach(EventInfo einfo in events)
			{
				values.Add(new String[] { }); // marks a new entry
				DescriptionAttribute descAttr = Attribute.GetCustomAttribute(einfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
				values.Add(new String[] {
						einfo.Name,
						GetTypeName(einfo.EventHandlerType),
						(descAttr == null ? "" : descAttr.Description)
					});
			}

			GenerateTable(sb, values);
		}

		static void GenerateMethodsDocs(Type type, StringBuilder sb)
		{

			List<MethodInfo> methods = new List<MethodInfo>(type.GetMethods(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
			));
			// Special name methods are the get_ and set_ of properties and the 
			// add_ and remove_ of event handlers
			methods = methods.FindAll(delegate (MethodInfo x) { return !x.IsSpecialName && !x.IsPrivate; });
			if(methods.Count == 0)
				return;

			methods.Sort(delegate (MethodInfo x, MethodInfo y)
			{
				return x.Name.CompareTo(y.Name);
			});

			sb.AppendLine("");
			sb.AppendLine("Methods");
			sb.AppendLine("^^^^^^^");

			List<String[]> values = new List<String[]>();
			values.Add(new String[] { "*Name*", "*Description*" });
			foreach(MethodInfo info in methods)
			{

				values.Add(new String[] { }); // marks a new entry
				DescriptionAttribute descAttr = Attribute.GetCustomAttribute(info, typeof(DescriptionAttribute)) as DescriptionAttribute;
				values.Add(new String[] {
						String.Format("``{0} {1} {2}({3})``",
							GetAccessLevel(info), GetTypeName(info.ReturnType), info.Name, MakeParameterList(info)),
						(descAttr == null ? "" : descAttr.Description)
					});
			}

			GenerateTable(sb, values);
		}

		private static String GetName(PropertyInfo pinfo)
		{
			String name = pinfo.Name;

			// Some name are so long they make the formatting strange.
			List<String[]> pairs = new List<String[]>(new String[][] {
				new String[] { "GroupWithItemCountSingularFormatOrDefault", "GroupWithItemCountSingularFormat OrDefault" },
				new String[] { "RenderNonEditableCheckboxesAsDisabled", "RenderNonEditableCheckboxes AsDisabled" },
				new String[] { "UpdateSpaceFillingColumnsWhenDraggingColumnDivider", "UpdateSpaceFillingColumnsWhenDragging ColumnDivider" },
				new String[] { "UnfocusedSelectedForeColorOrDefault", "UnfocusedHighlightForegroundColorOr Default" },
				new String[] { "UnfocusedSelectedBackColorOrDefault", "UnfocusedHighlightBackgroundColorOr Default" }

			});

			foreach(String[] pair in pairs)
			{
				if(name == pair[0])
				{
					name = pair[1];
					break;
				}
			}

			if(IsStatic(pinfo))
				name += " (static)";

			return name;
		}

		private static String GetTypeName(Type type)
		{
			//System.EventHandler`1[[BrightIdeasSoftware.CreateGroupsEventArgs, ObjectListView, Version=2.4.1.15087, Culture=neutral, PublicKeyToken=b1c5bf581481bcd4]]"
			Match eventHandlerMatch = Regex.Match(type.FullName, @"EventHandler.*\[.*?\.(.*?),");
			if(eventHandlerMatch.Success)
				return eventHandlerMatch.Groups[1].Value;

			Match genericTypeMatch = Regex.Match(type.FullName, @"\.([^.]+?)`1\[\[.+?\.(.+?),");
			if(genericTypeMatch.Success)
				return String.Format("{0}<{1}>", genericTypeMatch.Groups[1].Value, genericTypeMatch.Groups[2].Value);


			String[] fullName = type.FullName.Split('.');
			String last = fullName[fullName.Length - 1];
			return last;
		}

		private static String GetDefaultValue(DefaultValueAttribute attr)
			=> attr?.Value == null
				? String.Empty
				: attr.Value.ToString();


		private static String GetAccessLevel(PropertyInfo pInfo)
		{
			return GetAccessLevel(pInfo.GetGetMethod(true) ?? pInfo.GetSetMethod(true));
		}

		private static String GetAccessLevel(MethodInfo mInfo)
		{
			if(mInfo == null)
				return String.Empty;

			StringBuilder sb = new StringBuilder();
			MethodAttributes attrs = mInfo.Attributes;
			if((attrs & MethodAttributes.NewSlot) == MethodAttributes.NewSlot)
			{
				if(mInfo != mInfo.GetBaseDefinition())
					sb.Append("new ");
			}

			if((attrs & MethodAttributes.Public) == MethodAttributes.Public)
				sb.Append("public");
			else if((attrs & MethodAttributes.FamANDAssem) == MethodAttributes.FamANDAssem)
				sb.Append("internal");
			else if((attrs & MethodAttributes.Family) == MethodAttributes.Family)
				sb.Append("protected");
			else if((attrs & MethodAttributes.Private) == MethodAttributes.Private)
				sb.Append("private");

			if((attrs & MethodAttributes.Virtual) == MethodAttributes.Virtual)
				sb.Append(" virtual");

			return sb.ToString();
		}

		private static void GenerateTable(StringBuilder sb, List<String[]> values)
		{
			Int32[] valueMaxLength = new Int32[values[0].Length];
			foreach(String[] value in values)
			{
				for(Int32 i = 0; i < value.Length; i++)
				{
					valueMaxLength[i] = Math.Max(valueMaxLength[i], value[i].Length);
				}
			}
			for(Int32 i = 0; i < valueMaxLength.Length; i++)
				valueMaxLength[i] = Math.Max(20, valueMaxLength[i] + 2);

			String titleDivider = "";
			String rowDivider = "";
			for(Int32 i = 0; i < valueMaxLength.Length; i++)
			{
				titleDivider += new String('=', valueMaxLength[i]) + " ";
				rowDivider += new String('-', valueMaxLength[i]) + " ";
			}

			sb.AppendLine("");
			sb.AppendLine(titleDivider);

			// Print rows
			Int32 rowCount = 0;
			foreach(String[] value in values)
			{
				if(value.Length == 0)
				{
					if(rowCount <= 1)
						sb.AppendLine(titleDivider);
					else
						sb.AppendLine(rowDivider);
				} else
				{
					for(Int32 i = 0; i < value.Length; i++)
					{
						String formatString = String.Format("{0}0,-{1}{2} ", "{", valueMaxLength[i], "}");
						sb.AppendFormat(formatString, value[i]);
					}
					sb.AppendLine();
				}
				rowCount++;
			}
			sb.AppendLine(titleDivider);
		}

		private static String MakeParameterList(MethodInfo info)
		{
			StringBuilder sb = new StringBuilder();

			foreach(ParameterInfo parameter in info.GetParameters())
			{
				sb.Append(GetTypeName(parameter.ParameterType));
				sb.Append(" ");
				sb.Append(parameter.Name);
				sb.Append(", ");
			}

			// remove trailing comma and space
			if(sb.Length > 2)
				sb.Length -= 2;

			return sb.ToString();
		}
	}
}