/*!
	Copyright (C) 2008-2015 Kody Brown (kody@bricksoft.com).
	
	MIT License:
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to
	deal in the Software without restriction, including without limitation the
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
	sell copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
	DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Bricksoft.PowerCode;

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
		private ConsoleColor normalForeColor;
		private ConsoleColor normalBackColor;
		private ConsoleColor highlightForeColor;
		private ConsoleColor highlightBackColor;
		private ConsoleColor errorForeColor;
		private ConsoleColor errorBackColor;

		private static List<string> ConsoleColorNames = new List<string>(Enum.GetNames(typeof(ConsoleColor)));

		public pcolor( string[] arguments )
		{
			args = arguments;
		}

		public int Run()
		{
			normalForeColor = Console.ForegroundColor;
			normalBackColor = Console.BackgroundColor;
			highlightForeColor = ConsoleColor.Cyan;
			highlightBackColor = Console.BackgroundColor;
			errorForeColor = ConsoleColor.Red;
			errorBackColor = Console.BackgroundColor;

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

			// TODO change this project to use the Config settings.

			bool translateCR = false;
			bool translateLF = false;
			bool translateTab = false;
			bool wrapOutput = false;

			cmdlineBuilder = new StringBuilder();

			try {
				if (args.Length == 2 && (args[0].Equals("-s", StringComparison.CurrentCultureIgnoreCase) || args[0].Equals("-set", StringComparison.CurrentCultureIgnoreCase))) {
					ConsoleColor tmpColor;
					string arg;

					arg = args[1];
					if (arg.StartsWith("{") && arg.EndsWith("}")) {
						arg = arg.Substring(1, arg.Length - 2);
					}

					// Convert old DOS colors to .net ConsoleColor
					arg = ConvertDOSColors(arg); // TODO support bg

					if (int.TryParse(arg, out result)
							&& result >= (int)ConsoleColor.Black && result <= (int)ConsoleColor.White) {
						tmpColor = (ConsoleColor)result;
					} else if (ConsoleColorNames.Contains(arg, StringComparison.InvariantCultureIgnoreCase)) {
						tmpColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), arg, true);
					} else {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("error in -s argument: invalid color specified");
						Console.ForegroundColor = normalForeColor;
						return 10;
					}

					try {
						Console.ForegroundColor = tmpColor;
					} catch (Exception) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("error in -s argument: invalid color specified");
						Console.ForegroundColor = normalForeColor;
						return 10;
					}

					return 0;
				}

				if (ConsoleEx.IsInputRedirected) {
					// When piping text to pcolor, I'm assuming that there is only
					// one argument for pcolor and that is a color.
					cmdlineBuilder.Append(args[0]);
					cmdlineBuilder.Append(Console.In.ReadToEnd());
				} else {
					// Read all the content from the arguments.
					for (int argsi = 0; argsi < args.Length; argsi++) {
						string a = args[argsi];
						string al = a.ToLowerInvariant();

						if (al.Equals("-f") || al.Equals("-file")) {
							if (argsi <= args.Length - 2) {
								StringBuilder contents;

								argsi++;
								contents = new StringBuilder();

								if (!File.Exists(args[argsi])) {
									Console.ForegroundColor = errorForeColor;
									WriteLine(ConsoleColor.Red, "-f    the file was not found");
									DisplayUsage("-f");
									Console.ForegroundColor = normalForeColor;
									return 4;
								}

								if (contents.LoadFromFile(args[argsi])) {
									cmdlineBuilder.Append(contents.ToString());
								}
							} else {
								Console.ForegroundColor = errorForeColor;
								WriteLine(ConsoleColor.Red, "-f    is missing its file name");
								DisplayUsage("-f");
								Console.ForegroundColor = normalForeColor;
								return 4;
							}

						} else if (al.EndsWith("-escape") || al.EndsWith("-crlf") || al.EndsWith("-cr-lf") || al.EndsWith("-cr-lf-tab")) {
							translateCR = true;
							translateLF = true;
							translateTab = true;
						} else if (al.EndsWith("!escape") || al.EndsWith("!crlf") || al.EndsWith("!cr-lf") || al.EndsWith("!cr-lf-tab")) {
							translateCR = false;
							translateLF = false;
							translateTab = false;

						} else if (al.EndsWith("-cr")) {
							translateCR = true;
						} else if (al.EndsWith("!cr")) {
							translateCR = false;
						} else if (al.EndsWith("-lf")) {
							translateLF = true;
						} else if (al.EndsWith("!lf")) {
							translateLF = false;
						} else if (al.EndsWith("-tab")) {
							translateTab = true;
						} else if (al.EndsWith("!tab")) {
							translateTab = false;

						} else if (al.EndsWith("-wrap")) {
							wrapOutput = true;
						} else if (al.EndsWith("!wrap")) {
							wrapOutput = false;

						} else {
							// Convert hand-typed/specific linefeeds and tabs
							if (translateCR) {
								a = a.Replace("\\r\\n", "\r\n");
							}
							if (translateLF) {
								a = a.Replace("\\n", "\n");
							}
							if (translateTab) {
								a = a.Replace("\\t", "\t");
							}

							if (wrapOutput) {
								cmdlineBuilder.Append(Text.Wrap(a));
							} else {
								cmdlineBuilder.Append(a);
							}

							if (argsi < args.Length - 1 && !a.EndsWith("\n")) {
								cmdlineBuilder.Append(" ");
							}
						}
					}
				}

				cmdline = cmdlineBuilder.ToString();

				// Convert old DOS colors to .net ConsoleColor
				cmdline = ConvertDOSColors(cmdline);

				// Provide a default color.
				if (!cmdline.StartsWith("{")) {
					cmdline = "{" + normalForeColor.ToString() + "/" + normalBackColor.ToString() + "}" + cmdline;
				}

				WriteColoredString(cmdline);

			} catch (Exception ex) {
				// do nothing
				Console.Write(ex.Message);
				return 1;
			}

			Console.ForegroundColor = normalForeColor;

			return 0;
		}

		private Regex regAqua = new Regex(@".*[~\{]\{aqua\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regPurple = new Regex(@".*[~\{]\{purple\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regLightBlue = new Regex(@".*[~\{]\{light blue\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regLightGreen = new Regex(@".*[~\{]\{light green\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regLightAqua = new Regex(@".*[~\{]\{light Aqua\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regLightRed = new Regex(@".*[~\{]\{light red\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regLightPurple = new Regex(@".*[~\{]\{light purple\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regLightYellow = new Regex(@".*[~\{]\{light yellow\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private Regex regBrightWhite = new Regex(@".*[~\{]\{bright white\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private string ConvertDOSColors( string Text )
		{
			if (Text == null || Text.Length == 0) {
				return string.Empty;
			}

			Text = regAqua.Replace(Text, "{DarkCyan}");
			Text = regPurple.Replace(Text, "{DarkMagenta}");
			Text = regLightBlue.Replace(Text, "{Blue}");
			Text = regLightGreen.Replace(Text, "{Green}");
			Text = regLightAqua.Replace(Text, "{Cyan}");
			Text = regLightRed.Replace(Text, "{Red}");
			Text = regLightPurple.Replace(Text, "{Magenta}");
			Text = regLightYellow.Replace(Text, "{Yellow}");
			Text = regBrightWhite.Replace(Text, "{White}");

			return Text;
		}

		private void DisplayUsage( string Topic ) { DisplayUsage(Topic, false); }

		private void DisplayUsage( string Topic, bool ShowHidden )
		{
			string text = "",
				text2 = "";

			Console.ForegroundColor = highlightForeColor;
			Console.WriteLine("color.exe");
			Console.ForegroundColor = normalForeColor;
			Console.WriteLine(Text.Wrap("Copyright (C) 2010-2015 Kody Brown."));
			Console.WriteLine(Text.Wrap("Released under the MIT license. Use at your own risk."));
			Console.WriteLine();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("USAGE:");
			Console.ForegroundColor = normalForeColor;
			Console.WriteLine();
			Console.WriteLine("  color.exe [-s] [-f filename] ...");
			Console.WriteLine();

			Console.WriteLine(Text.Wrap("   -cr                 Convert the char literals `\\`+`r` to `\\r`.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   !cr                 Do not convert the char literals from `\\`+`r` to `\\r`.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   -lf                 Convert the char literals `\\`+`n` to `\\n`.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   !lf                 Do not convert the char literals from `\\`+`n` to `\\n`.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   -tab                Convert the char literals `\\`+`t` to `\\t`.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   !tab                Do not convert the char literals from `\\`+`t` to `\\t`.", 0, 0, 23));
			Console.WriteLine();
			Console.WriteLine(Text.Wrap("   -crlf               Converts cr, lf, and tab literals.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   !crlf               Do not convert any.", 0, 0, 23));
			Console.WriteLine();
			Console.WriteLine(Text.Wrap("   -wrap               Wraps the output to the console width. Wraps each argument by itself.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   !wrap               Turns wrapping off.", 0, 0, 23));

			if (Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "c", "color")) {
				Console.ForegroundColor = highlightForeColor;
			}
			Console.WriteLine();
			Console.WriteLine(Text.Wrap("   {color}             The color to use. Use the color value (number) or name of the color (case in-sensitive). All text following the color will be displayed in that color.\n\nTo specify the foreground and background colors, separate the colors with a forward slash `{fore/back}`. If you want to only change the background color, remove the forecolor, but leave the forward-slash `{/back}.", 0, 0, 23));
			Console.ForegroundColor = normalForeColor;

			Console.WriteLine();
			Console.WriteLine(Text.Wrap("The flags above are all 'chainable', meaning they can be used repeatedly throughout the command-line arguments. See the examples for more details.", -6, 6));

			//if (ShowHidden || Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "f", "file")) {
			//	Console.ForegroundColor = highlightColor;
			Console.WriteLine();
			Console.WriteLine(Text.Wrap("   -file \"filename\"    Displays the file contents. Colors are read the same as if entered on the command-line.", 0, 0, 23));
			//	Console.ForegroundColor = normalColor;
			//}

			if (Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "f", "file")) {
				Console.ForegroundColor = highlightForeColor;
			}
			Console.WriteLine();
			Console.WriteLine(Text.Wrap("   -s color            Apply the color to the console, not just the text being output.", 0, 0, 23));
			Console.ForegroundColor = normalForeColor;

			Console.WriteLine();
			Console.WriteLine(Text.Wrap("   /?                  display help.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   /? examples         display examples.", 0, 0, 23));
			Console.WriteLine(Text.Wrap("   /? colors           display colors.", 0, 0, 23));

			if (ShowHidden || Topic.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, "c", "colors")) {
				Console.WriteLine();
				Console.ForegroundColor = highlightForeColor;
				Console.WriteLine("Colors:");
				Console.ForegroundColor = normalForeColor;
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
				Console.ForegroundColor = highlightForeColor;
				Console.WriteLine("Examples:");
				Console.ForegroundColor = normalForeColor;
				Console.WriteLine();

				text = "{Red} \"This line is red.\"";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text.Replace("\"", "") + "\n");
				Console.WriteLine();

				text = "{Red} This line is red.";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text + "\n");
				Console.WriteLine();

				text = "{Red}This line is red.";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text + "\n");
				Console.WriteLine();

				text = "{DarkMagenta}This line is DarkMagenta.";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text + "\n");
				Console.WriteLine();

				text = "{5}So is this line.";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text + "\n");
				Console.WriteLine();

				text = "{Green}The \"quotes\" will not be displayed.";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text.Replace("\"", "") + "\n");
				Console.WriteLine();

				text = "{Yellow}Unless you do \"\"\"this\"\"\" or \\\"this\\\" instead.";
				Console.WriteLine(">pcolor.exe " + text);
				WriteColoredString(text.Replace("\\\"", "\"").Replace("\"\"\"", "\"") + "\n");
				Console.WriteLine();

				text = "{Red} \"Red\" {White} \"White\" {Blue} \"Blue\"";
				Console.WriteLine(">pcolor.exe " + text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t"));
				WriteColoredString(text + "\n");
				Console.WriteLine();

				text = "{Red} \"Red\" {White} \"White\" {Blue} \"Blue\"";
				Console.WriteLine(">pcolor.exe " + text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t"));
				WriteColoredString(text + "\n");
				Console.WriteLine();

				text = "{Red}\"Red\" {White}\"White\" {Blue}\"Blue\"";
				Console.WriteLine(">pcolor.exe " + text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t"));
				WriteColoredString(text + "\n");
				Console.WriteLine();

				//pcolor.exe --crlf "{Red}\tRed\n{White}\tWhite\n" -!crlf "{Gray}\tand..\n" "{Blue}\tBlue\n."
				text = "{Red}\tRed\n{White}\tWhite\n";
				text2 = "{Gray}\tand..\n\t{Blue}\tBlue\n.";
				Console.WriteLine(">pcolor.exe --crlf \"" + text.Replace("\n", "\\n").Replace("\t", "\\t") + "\" -!crlf \"" + text2.Replace("\n", "\\n").Replace("\t", "\\t") + "\"");
				WriteColoredString(text + text2.Replace("\n", "\\n").Replace("\t", "\\t") + "\n");
				Console.WriteLine();

				text = @"
{White/Blue} * * * * * * {White/Red}                          
{White/Blue}  * * * * *  {/White}                          
{White/Blue} * * * * * * {White/Red}                          
{White/Blue}  * * * * *  {/White}                          
{White/Blue} * * * * * * {White/Red}                          
{/White}                                       
{/Red}                                       
{/White}                                       
{/Red}                                       
{/White}                                       
{/Red}                                       {/Black} ".Trim();
				Console.WriteLine(">pcolor.exe -crlf \"" + text.Replace("\r\n", "\n").Replace("\n", "\\n") + "\"");
				WriteColoredString(text + "\n");
			}

			Console.ForegroundColor = normalForeColor;
		}

		/* ----- PowerCode ------------------------------------------------------------------------------------------------------------------------------------------------------ */

		private void Write( ConsoleColor foreColor, string value )
		{
			ConsoleColor backupForeColor = Console.ForegroundColor;
			Console.ForegroundColor = foreColor;
			Console.Write(value);
			Console.ForegroundColor = backupForeColor;
		}

		private void Write( ConsoleColor foreColor, string format, params object[] parameters )
		{
			Write(foreColor, Console.BackgroundColor, format, parameters);
		}

		private void Write( ConsoleColor foreColor, ConsoleColor backColor, string value )
		{
			ConsoleColor backupForeColor = Console.ForegroundColor;
			ConsoleColor backupBackColor = Console.BackgroundColor;
			Console.ForegroundColor = foreColor;
			Console.BackgroundColor = backColor;
			Console.Write(value);
			Console.ForegroundColor = backupForeColor;
			Console.BackgroundColor = backupBackColor;
		}

		private void Write( ConsoleColor foreColor, ConsoleColor backColor, string format, params object[] parameters )
		{
			Write(foreColor, backColor, string.Format(format, parameters));
		}

		private void WriteLine( ConsoleColor foreColor, string value )
		{
			Write(foreColor, Console.BackgroundColor, value);
			Console.WriteLine();
		}

		private void WriteLine( ConsoleColor foreColor, ConsoleColor backColor, string value )
		{
			Write(foreColor, backColor, value);
			Console.WriteLine();
		}

		private void WriteLine( ConsoleColor foreColor, string format, params object[] parameters )
		{
			Write(foreColor, Console.BackgroundColor, format, parameters);
			Console.WriteLine();
		}

		private void WriteLine( ConsoleColor foreColor, ConsoleColor backColor, string format, params object[] parameters )
		{
			Write(foreColor, backColor, format, parameters);
			Console.WriteLine();
		}

		/* ----- ColoredString ------------------------------------------------------------------------------------------------------------------------------------------------------ */

		/// <summary>
		/// Provides a way to write a line to the console using several colors, indicated by color tags
		/// For instance: "This is <Blue>blue</Blue>, while this is <Green>green</Green>"
		/// </summary>
		private class ColoredString
		{
			public ConsoleColor ForeColor { get; set; }
			public ConsoleColor BackColor { get; set; }
			public string Text { get; set; }
			public ColoredString() { }
			public ColoredString( ConsoleColor ForeColor, string Text ) : this(ForeColor, Console.BackgroundColor, Text) { }
			public ColoredString( ConsoleColor ForeColor, ConsoleColor BackColor, string Text )
			{
				this.ForeColor = ForeColor;
				this.BackColor = BackColor;
				this.Text = Text;
			}

			private static List<string> ConsoleColorNames = new List<string>(Enum.GetNames(typeof(ConsoleColor)));
			private static Regex regForeColor = new Regex(@"\{(?:[A-Za-z0-9]*)/?(?:[A-Za-z0-9]*)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			//  rstr = @"(?:(/[^:]*)+/?)$";

			public static List<ColoredString> Parse( string Text, params object[] Parameters )
			{
				List<ColoredString> coloredStrings;
				//List<string> sections;
				//string[] parts;

				if (Parameters != null && Parameters.Length > 0) {
					Text = string.Format(Text, Parameters);
				}

				coloredStrings = new List<ColoredString>();

				Match m;
				int lastIndex = 0;
				ConsoleColor
					lastFore = Console.ForegroundColor,
					lastBack = Console.BackgroundColor;
				string
					tmpText = "",
					color = "";
				string[] colorx;

				m = regForeColor.Match(Text);
				while (m.Success) {
					// NOTE: I'm breaking on the colors, so I must output the text before the color change..
					tmpText = Text.Substring(lastIndex, m.Index - lastIndex);
					if (tmpText.Length > 0) {
						coloredStrings.Add(new ColoredString(lastFore, lastBack, tmpText));
					}

					color = m.Value.TrimStart('{').TrimEnd('}');
					if (color.IndexOf('/') > -1) {
						colorx = color.Split('/');
						if (colorx.Length == 2) {
							if (colorx[0].Length > 0) {
								if (!GetConsoleColor(colorx[0], out lastFore)) {
									// Not a valid structure, so output it as plain-text..
									coloredStrings.Add(new ColoredString(lastFore, lastBack, m.Value));
								}
							}
							if (colorx[1].Length > 0) {
								if (!GetConsoleColor(colorx[1], out lastBack)) {
									// Not a valid structure, so output it as plain-text..
									coloredStrings.Add(new ColoredString(lastFore, lastBack, m.Value));
								}
							}
						} else {
							// Not a valid structure, so output it as plain-text..
							coloredStrings.Add(new ColoredString(lastFore, lastBack, m.Value));
						}
					} else {
						if (!GetConsoleColor(color, out lastBack)) {
							// Not a valid structure, so output it as plain-text..
							coloredStrings.Add(new ColoredString(lastFore, lastBack, m.Value));
						}
					}

					lastIndex = m.Index + m.Length;
					m = m.NextMatch();
				}

				if (lastIndex < Text.Length) {
					tmpText = Text.Substring(lastIndex);
					if (tmpText.Length > 0) {
						coloredStrings.Add(new ColoredString(lastFore, lastBack, tmpText));
					}
				}

				return coloredStrings;
			}

			public static bool GetConsoleColor( string ColorNameOrNumber, out ConsoleColor Color )
			{
				int val;
				if (int.TryParse(ColorNameOrNumber, out val) && val >= (int)ConsoleColor.Black && val <= (int)ConsoleColor.White) {
					Color = (ConsoleColor)val;
					return true;
				} else if (ConsoleColorNames.Contains(ColorNameOrNumber, StringComparison.InvariantCultureIgnoreCase)) {
					Color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), ColorNameOrNumber, true);
					return true;
				}
				Color = ConsoleColor.Black;
				return false;
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
				Write(coloredString.ForeColor, coloredString.BackColor, coloredString.Text);
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
				WriteLine(coloredString.ForeColor, coloredString.Text);
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
