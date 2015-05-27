pcolor
======

output colored text lines to the console

    pcolor.exe [-s] -color ["]text {color}text["] -color ["]text text["] ...
    Copyleft (L) 2010. Free to use and modify. Use at your own risk.

       -s color            Apply the color to the console. (not just the text being output.)
       {color}             The color to use. Use the color value (number) or name of the color (case in-sensitive).
                           All text following the color will be displayed in that color.

       /?                  display help.
       /? examples         display examples.
       /? colors           display colors.

    Colors:

        0 = Black (made visible)
        1 = DarkBlue
        2 = DarkGreen
        3 = DarkCyan
        4 = DarkRed
        5 = DarkMagenta
        6 = DarkYellow
        7 = Gray
        8 = DarkGray
        9 = Blue
       10 = Green
       11 = Cyan
       12 = Red
       13 = Magenta
       14 = Yellow
       15 = White
       
    Examples:

        pcolor.exe {Red} "This line is red."

    This line is red.

        pcolor.exe {Green}This line is green.

    This line is green.

        pcolor.exe {DarkMagenta} This line is DarkMagenta.

    This line is DarkMagenta.

        pcolor.exe {5} So is this line.
    So is this line.

        pcolor.exe {Green} The "quotes" will not be displayed.

    The quotes will not be displayed.

        pcolor.exe {Yellow} Unless you do """this""" instead.

    Unless you do "this" instead.

        pcolor.exe {DarkGray} You can do {White}M{DarkGray}ultiple colors on each line.

    You can do Multiple colors on each line.

        pcolor.exe {White} Quotes can be used to create space and formatting.

    Quotes can be used to create space and formatting.

        pcolor.exe {Cyan} Use quotes and `\n` to create custom formatting and new lines such as: "\n  {Red}Red" "\n   {White}  White" "\n    {Blue}  Blue"

    Use quotes and `\n` to create custom formatting and new lines such as:
      Red
       White
        Blue
