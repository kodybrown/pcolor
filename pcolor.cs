using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bricksoft.DosToys
{
	public class pcolor
	{
		public static int Main( string[] arguments )
		{
			pcolor app;

			app = new pcolor(arguments);
			return app.Run();
		}

		private string[] args;
		private ConsoleColor normalColor;
		private ConsoleColor highlightColor;
		private ConsoleColor errorColor;

		public pcolor( string[] arguments )
		{
			args = arguments;
		}

		public int Run()
		{
			normalColor = Console.ForegroundColor;
			highlightColor = ConsoleColor.Cyan;
			errorColor = ConsoleColor.Red;

			if (args.Length == 0 || (args.Length > 0 && args[0].EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "/?", "/h", "/help"))) {
				DisplayUsage(args.Length > 1 ? args[1] : null);
				return 0;
			}
			if (args[0].EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "/hidden")) {
				DisplayUsage(null, true);
				return 0;
			}

			string cmdline;
			StringBuilder cmdlineBuilder;
			int result;

			cmdlineBuilder = new StringBuilder();

			try {
				if (args.Length == 2 && (args[0].Equals("-s", StringComparison.CurrentCultureIgnoreCase) || args[0].Equals("-set", StringComparison.CurrentCultureIgnoreCase))) {
					ConsoleColor tmpColor;
					string[] colors;
					string arg;

					colors = Enum.GetNames(typeof(ConsoleColor));

					arg = args[1];
					if (arg.StartsWith("{") && arg.EndsWith("}")) {
						arg = arg.Substring(1, arg.Length - 2);
					}

					// Convert old DOS colors to .net ConsoleColor
					arg = ConvertDOSColors(arg);

					if (int.TryParse(arg, out result)) {
						tmpColor = (ConsoleColor)result;
					} else if (colors.Contains(arg, StringComparison.InvariantCultureIgnoreCase)) {
						tmpColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), arg, true);
					} else {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("error in -s argument: invalid color specified");
						Console.ForegroundColor = normalColor;
						return 10;
					}

					try {
						Console.ForegroundColor = tmpColor;
					} catch (Exception) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("error in -s argument: invalid color specified");
						Console.ForegroundColor = normalColor;
						return 10;
					}

					return 0;
				}

				for (int index = 0; index < args.Length; index++) {
					if (args[index].Equals("-f", StringComparison.CurrentCultureIgnoreCase) || args[index].Equals("-file", StringComparison.CurrentCultureIgnoreCase)) {
						if (index <= args.Length - 2) {
							StringBuilder contents;

							index++;
							contents = new StringBuilder();

							if (!File.Exists(args[index])) {
								Console.ForegroundColor = errorColor;
								WriteLine(ConsoleColor.Red, "-f    the file was not found");
								DisplayUsage("-f");
								Console.ForegroundColor = normalColor;
								return 4;
							}

							if (contents.LoadFromFile(args[index])) {
								cmdlineBuilder.Append(contents.ToString());
							}
						} else {
							Console.ForegroundColor = errorColor;
							WriteLine(ConsoleColor.Red, "-f    is missing its file name");
							DisplayUsage("-f");
							Console.ForegroundColor = normalColor;
							return 4;
						}
					} else {
						cmdlineBuilder.Append(args[index]);
						if (index < args.Length - 1 && !args[index].EndsWith("\n")) {
							cmdlineBuilder.Append(" ");
						}
					}
				}

				cmdline = cmdlineBuilder.ToString();

				// Convert hand-typed/specific linefeeds and tabs
				cmdline = cmdline.Replace("\\r\\n", "\r\n");
				cmdline = cmdline.Replace("\\n", "\n");
				cmdline = cmdline.Replace("\\t", "\t");

				// Convert old DOS colors to .net ConsoleColor
				cmdline = ConvertDOSColors(cmdline);

				// Provide a default color.
				if (!cmdline.StartsWith("{")) {
					cmdline = "{" + normalColor.ToString() + "}" + cmdline;
				}

				WriteColoredString(cmdline);

			} catch (Exception ex) {
				// do nothing
				Console.Write(ex.Message);
				return 1;
			}

			Console.ForegroundColor = normalColor;

			return 0;
		}

		private string ConvertDOSColors( string Text )
		{
			if (Text == null || Text.Length == 0) {
				return string.Empty;
			}

			Text = Regex.Replace(Text, @".*[~\{]\{aqua\}", "{DarkCyan}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{purple\}", "{DarkMagenta}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{light blue\}", "{Blue}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{light green\}", "{Green}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{light Aqua\}", "{Cyan}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{light red\}", "{Red}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{light purple\}", "{Magenta}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{light yellow\}", "{Yellow}", RegexOptions.IgnoreCase);
			Text = Regex.Replace(Text, @".*[~\{]\{bright white\}", "{White}", RegexOptions.IgnoreCase);

			return Text;
		}

		private void DisplayUsage( string Topic ) { DisplayUsage(Topic, false); }

		private void DisplayUsage( string Topic, bool ShowHidden )
		{

			Console.ForegroundColor = highlightColor;
			Console.WriteLine("color.exe [-s] -color [\"]text {color}text[\"] -color [\"]text text[\"] ...");
			Console.ForegroundColor = normalColor;
			Console.WriteLine("Copyleft (L) 2010. Free to use and modify. Use at your own risk.");
			Console.WriteLine();

			if (Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "f", "file")) {
				Console.ForegroundColor = highlightColor;
			}
			Console.WriteLine("   -s color            Apply the color to the console. (not just the text being output.)");
			Console.ForegroundColor = normalColor;

			if (Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "c", "color")) {
				Console.ForegroundColor = highlightColor;
			}
			Console.WriteLine("   {color}             The color to use. Use the color value (number) or name of the color (case in-sensitive).");
			Console.WriteLine("                       All text following the color will be displayed in that color.");
			Console.ForegroundColor = normalColor;

			if (ShowHidden || Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "f", "file")) {
				Console.ForegroundColor = highlightColor;
				Console.WriteLine("   -file \"filename\"  Displays the file contents. Colors are read as if entered on the command-line.");
				Console.ForegroundColor = normalColor;
			}

			Console.WriteLine();
			Console.WriteLine("   /?                  display help.");
			Console.WriteLine("   /? examples         display examples.");
			Console.WriteLine("   /? colors           display colors.");

			if (ShowHidden || Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "c", "colors")) {
				Console.WriteLine();
				Console.ForegroundColor = highlightColor;
				Console.WriteLine("Colors:");
				Console.ForegroundColor = normalColor;
				Console.WriteLine();

				string[] names = Enum.GetNames(typeof(ConsoleColor));
				Array values = Enum.GetValues(typeof(ConsoleColor));
				ConsoleColor color;

				for (int i = 0; i < names.Length; i++) {
					color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), names[i]);
					if (Console.BackgroundColor == color) {
						Console.WriteLine("   {1} = {0} (made visible)", names[i], ((int)color).ToString().PadLeft(2, ' ')); //values.GetValue(i)
					} else {
						WriteLine(color, "   {1} = {0}", names[i], ((int)color).ToString().PadLeft(2, ' ')); //values.GetValue(i)
					}
				}
				Console.WriteLine();
			}

			if (ShowHidden || Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "e", "ex", "example", "examples")) {
				Console.WriteLine();
				Console.ForegroundColor = highlightColor;
				Console.WriteLine("Examples:");
				Console.ForegroundColor = normalColor;
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -Red \"This line is red.\"");
				WriteLine(ConsoleColor.Red, "This line is red.");
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -Red This line is red.");
				WriteLine(ConsoleColor.Red, "This line is red.");
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -DarkMagenta This line is DarkMagenta.");
				WriteLine(ConsoleColor.DarkMagenta, "This line is DarkMagenta.");
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -4 So is this line.");
				WriteLine(ConsoleColor.DarkMagenta, "So is this line.");
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -Green The \"quotes\" will not be displayed.");
				WriteLine(ConsoleColor.Green, "The quotes will not be displayed.");
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -Yellow Unless you do \"\"\"this\"\"\" instead.");
				WriteLine(ConsoleColor.Yellow, "Unless you do \"this\" instead.");
				Console.WriteLine();
				Console.WriteLine("pcolor.exe -Red \"Red\" -White \"White\" -Blue \"Blue\".");
				Write(ConsoleColor.Red, "Red ");
				Write(ConsoleColor.White, "White ");
				WriteLine(ConsoleColor.Blue, "Blue");
				Console.WriteLine();
			}

			Console.ForegroundColor = normalColor;
		}

		/* ----- PowerCode ------------------------------------------------------------------------------------------------------------------------------------------------------ */

		private void Write( ConsoleColor foreColor, string value )
		{
			ConsoleColor backupForeColor = Console.ForegroundColor;
			Console.ForegroundColor = foreColor;
			Console.Write(value);
			Console.ForegroundColor = backupForeColor;
		}

		private void Write( ConsoleColor foreColor, string format, params object[] parameters ) { Write(foreColor, Console.BackgroundColor, format, parameters); }

		private void Write( ConsoleColor foreColor, ConsoleColor backColor, string format, params object[] parameters )
		{
			ConsoleColor backupForeColor = Console.ForegroundColor;
			ConsoleColor backupBackColor = Console.BackgroundColor;
			Console.ForegroundColor = foreColor;
			Console.BackgroundColor = backColor;
			Console.Write(format, parameters);
			Console.ForegroundColor = backupForeColor;
			Console.BackgroundColor = backupBackColor;
		}

		private void WriteLine( ConsoleColor foreColor, string value )
		{
			Write(foreColor, Console.BackgroundColor, value);
			Console.WriteLine();
		}

		private void WriteLine( ConsoleColor foreColor, string format, params object[] parameters )
		{
			Write(foreColor, Console.BackgroundColor, format, parameters);
			Console.WriteLine();
		}

		/* ----- ColoredString ------------------------------------------------------------------------------------------------------------------------------------------------------ */

		/// <summary>
		/// Provides a way to write a line to the console using several colors, indicated by color tags
		/// For instance: "This is <Blue>blue</Blue>, while this is <Green>green</Green>"
		/// </summary>
		private class ColoredString
		{
			public ConsoleColor Color { get; set; }
			public string Text { get; set; }
			public ColoredString() { }
			public ColoredString( ConsoleColor Color, string Text ) { this.Color = Color; this.Text = Text; }

			public static List<ColoredString> Parse( string Text, params object[] Parameters )
			{
				List<ColoredString> coloredStrings;
				List<string> sections;
				string[] parts;

				if (Parameters != null && Parameters.Length > 0) {
					Text = string.Format(Text, Parameters);
				}

				coloredStrings = new List<ColoredString>();

				foreach (string color in Enum.GetNames(typeof(ConsoleColor))) {
					//if (Text.IndexOf("{" + color + "}") > -1) {
					//   Text = Text.Replace("{" + color + "}", "\r\n@={" + color + "}");
					//}
					Text = Regex.Replace(Text, @"\{" + color + @"\}", "\r\n@={" + color + @"}", RegexOptions.IgnoreCase);
				}

				sections = new List<string>(Text.Split(new string[] { "\r\n@=" }, StringSplitOptions.RemoveEmptyEntries));

				foreach (string val in sections) {
					parts = val.Split(new char[] { '}' }, 2);
					parts[0] = parts[0].Substring(1); // remove the preceding '{'
					coloredStrings.Add(new ColoredString((ConsoleColor)Enum.Parse(typeof(ConsoleColor), parts[0]), parts[1]));
				}

				return coloredStrings;
			}
		}

		/// <summary>
		/// Writes a ColoredString to the console.
		/// </summary>
		/// <param name="Text"></param>
		/// <param name="Parameters"></param>
		public void WriteColoredString( string Text, params object[] Parameters )
		{
			List<ColoredString> coloredStrings;

			coloredStrings = ColoredString.Parse(Text, Parameters);

			foreach (ColoredString coloredString in coloredStrings) {
				Write(coloredString.Color, coloredString.Text);
			}
		}

		/// <summary>
		/// Writes a ColoredString to the console.
		/// </summary>
		/// <param name="Text"></param>
		/// <param name="Parameters"></param>
		public void WriteColoredStringLine( string Text, params object[] Parameters )
		{
			List<ColoredString> coloredStrings;

			coloredStrings = ColoredString.Parse(Text, Parameters);

			foreach (ColoredString coloredString in coloredStrings) {
				WriteLine(coloredString.Color, coloredString.Text);
			}
		}

	}

	/* ----- String Extensions ------------------------------------------------------------------------------------------------------------------------------------------------------ */

	internal static class StringExt
	{

		public static bool LoadFromFile( this StringBuilder me, string fileName )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (fileName == null || (fileName = fileName.Trim()).Length == 0) {
				throw new ArgumentNullException("fileName");
			}

			me.Length = 0;

			try {
				if (!File.Exists(fileName)) {
					return false;
				}

				using (StreamReader reader = File.OpenText(fileName)) {
					while (!reader.EndOfStream) {
						me.AppendLine(reader.ReadLine());
					}
					reader.Close();
				}
			} catch (Exception) {

			}

			return true;
		}

		public static bool EqualsOneOf( this string me, params string[] parameters ) { return me.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, parameters); }

		public static bool EqualsOneOf( this string me, StringComparison StringComparison, params string[] parameters )
		{
			if (me == null) {
				return false;
			}
			if (parameters == null || parameters.Length == 0) {
				return false;
			}

			foreach (string param in parameters) {
				if (me.Equals(param, StringComparison)) {
					return true;
				}
			}
			return false;
		}

		public static bool Contains( this string[] me, string Value, StringComparison StringComparison ) { return new List<string>(me).Contains(Value, StringComparison); }

		public static bool Contains( this List<string> me, string Value, StringComparison StringComparison )
		{
			string item;

			if (me == null) {
				throw new ArgumentNullException("List"); // L10N=Safe
			}

			for (int i = 0; i < me.Count; i++) {
				item = me[i];
				if (item.Contains(Value, StringComparison)) {
					return true;
				}
			}

			return false;
		}

		public static bool Contains( this string me, string Value, StringComparison StringComparison )
		{
			if (me == null && Value == null) {
				return true;
			}
			if (me == null) {
				throw new ArgumentNullException("me"); // L10N=Safe
			}
			if (Value == null) {
				return false;
				//throw new ArgumentNullException("Value"); // L10N=Safe
			}

			return me.IndexOf(Value, StringComparison) > -1;
		}

	}
}
