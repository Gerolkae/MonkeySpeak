--(0:0) when the script starts,
		--(5:110) load library from file {Monkeyspeak.WindowsForms.Lib.dll}. (example Monkeyspeak.dll)
		--(5:5100) show message box with message {Loaded Monkeyspeak.WindowsForms.Lib.dll!}.
		--(5:5101) create windows form with title {New Form} and set the id to variable %newForm.
		--(5:102) print {Created new Windows Form %newForm} to the console.

(0:0) when the script starts,
	(5:100) set variable %file to {test.txt}.
	(5:102) print {%file} to the console.

(0:0) when the script starts,
	(1:200) and the file {%file} exist,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak!} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.