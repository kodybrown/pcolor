pcolor
======

Output colored text lines to the console

    > pcolor /?
    color.exe [-s] [-f filename] ...
    Copyright (C) 2010-2015 Kody Brown.
    Released under the MIT license. Use at your own risk.

       -cr                 Convert the char literals `\`+`r` to `\r`.
       !cr                 Do not convert the char literals from `\`+`r` to `\r`.
       -lf                 Convert the char literals `\`+`n` to `\n`.
       !lf                 Do not convert the char literals from `\`+`n` to `\n`.
       -tab                Convert the char literals `\`+`t` to `\t`.
       !tab                Do not convert the char literals from `\`+`t` to `\t`.

       -crlf               Converts cr, lf, and tab literals.
       !crlf               Do not convert any.

       -wrap               Wraps the output to the console width. Wraps each
                           argument by itself.
       !wrap               Turns wrapping off.

       {color}             The color to use. Use the color value (number) or name
                           of the color (case in-sensitive). All text following the
                           color will be displayed in that color.
                       
                           To specify the foreground and background colors,
                           separate the colors with a forward slash `{fore/back}`.
                           If you want to only change the background color, remove
                           the forecolor, but leave the forward-slash `{/back}.

          The flags above are all 'chainable', meaning they can be used
          repeatedly throughout the command-line arguments. See the examples
          for more details.

       -file "filename"    Displays the file contents. Colors are read the same as
                           if entered on the command-line.

       -s color            Apply the color to the console, not just the text being
                           output.

       /?                  display help.
       /? examples         display examples.
       /? colors           display colors.

    >pcolor /? colors
    ...
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

    >pcolor /? examples
    ...
    Examples:
    
    >pcolor.exe {Red} "This line is red."
     This line is red.
    
    >pcolor.exe {Red} This line is red.
     This line is red.
    
    >pcolor.exe {Red}This line is red.
    This line is red.
    
    >pcolor.exe {DarkMagenta}This line is DarkMagenta.
    This line is DarkMagenta.
    
    >pcolor.exe {4}So is this line.
    {4}So is this line.
    
    >pcolor.exe {Green}The "quotes" will not be displayed.
    The quotes will not be displayed.
    
    >pcolor.exe {Yellow}Unless you do """this""" or \"this\" instead.
    Unless you do "this" or "this" instead.
    
    >pcolor.exe {Red} \"Red\" {White} \"White\" {Blue} \"Blue\"
     "Red"  "White"  "Blue"
    
    >pcolor.exe {Red} \"Red\" {White} \"White\" {Blue} \"Blue\"
     "Red"  "White"  "Blue"
    
    >pcolor.exe {Red}\"Red\" {White}\"White\" {Blue}\"Blue\"
    "Red" "White" "Blue"
    
    >pcolor.exe --crlf "{Red}\tRed\n{White}\tWhite\n" -!crlf "{Gray}\tand..\n\t{Blue}\tBlue\n."
        Red
        White
    \tand..\n\t\tBlue\n.
    
    >pcolor.exe -crlf "{White/Blue} * * * * * * {White/Red}                          \n{White/Blue}  * * * * *  {/White}                          \n{White/Blue} * * * * * * {White/Red}                          \n{White/Blue}  * * * * *  {/White}                          \n{White/Blue} * * * * * * {White/Red}                          \n{/White}                                       \n{/Red}                                       \n{/White}                                       \n{/Red}                                       \n{/White}                                       \n{/Red}                                       {/Black}"
     * * * * * *                           
      * * * * *                            
     * * * * * *                           
      * * * * *                            
     * * * * * *                           
                                           
                                           
                                           
                                           
                                           
                                           
    (has colored backgrounds in the command prompt window..)
