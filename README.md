VineScript is a language designed for interactive storytelling and dialogues in games.

This is still work in progress and in no way ready to be used!


Syntax example:
```
/*
This is a multiline comment
*/

// this is a line comment

This is text.

// Here's a variable definition:
<< set myvar to 42 >>

// There's 4 different types and 2 data structures:
<< set myInt = 22 >>
<< set myNumber = 22.022 >>
<< set myBool = true >> // could also be false
<< set myString = "This is a string" >>
<< set myArray = [1, 2, 3.2, false, myString] >>
<< set myDictionnary = {
    "Vine": "Templating langague written in C#",
    "Libs used": "Antlr 4.5"
} >>

Let's do a condition:
<< if myvar > 0 >>
	Let's print it: {{ myvar }}
<< end >>

// The type of a variable is dynamic and can be changed:
<< set myvar = "Hello, World!" >>

Calling functions:
<< if IsInt(myvar) >>
	"{{ myvar }}" is an integer!
<< elif IsString(myvar) >>
	"{{ myvar }}" is a string!
<< else >>
	"{{ myvar }}" is something else!
<< end >>

// All variables are global. When you're done with a variable, you can unset it:
<< unset myvar >>

Loops:
<< set prev = 0, result = 0 >>
<< for i in [1, 2, 4] >>
	<< set prev = result, result += i >>
	{{ prev }} + {{ i }} = {{ result }}
<< end >>
```