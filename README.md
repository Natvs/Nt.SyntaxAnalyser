# Syntax Analyser

## Introduction
This project is following the [SyntaxParser project](https://github.com/Natvs/Nt.SyntaxParser). 
While the syntax parser reads a grammar from a string or file and create a grammar structure, this project aims to produce a syntax tree given a grammar and a source code file.

In order to do so, it tokenizes the source code file, then applies a descending parsing algorithm and produces the syntax tree. 
For that analysis, the grammar must be LL(1). This project applies several refactorisation it LL(1) compliant. 
However, as it is not guaranteed that the grammar is LL(1) after applying these methods, further manual restructuration of the grammar might be required at some point.

## Usage of the syntax analyser
With your grammar file and your source code file ready, you can use the syntax analyser with the following code:

```csharp
using Nt.Syntax;

// Create a reader
var reader = SourceReader.CreateFromFile("path/to/the/grammar/file.txt", [' ', '\n'], ["{", "}", ";"], [';', '}']);

// Analyse the source code
var result = reader.AnalyseFile("path/to/the/source/code.txt");
```

Here are the detailed steps:
1. Creating the grammar: here the method `CreateFromFile` is used, but you can also use `CreateFromString` or `CreateFromGrammar`.
	- `separators` is a list of characters to be considered as separators in the source code.
	- `symbols`is a list of strings representing symbols in the source code.
	- `checkpoints` is a list of symbols to reach in case of errors, so that the analysing can continue.
2. Analysing the source code: here the method `AnalyseFile` is used, but you can also use `AnalyseString`.




You may need to write your own grammar file. If so, please refer to the [grammar file syntax](https://github.com/Natvs/Nt.SyntaxParser/blob/master/Doc/Grammar.md) documentation.

## Analyse result
The `Analyse` method returns an `AnalyseResult` structure that contains datas about the analysis, including the list of errors encountered during the analysis.

The fields of this structure are:
- `SyntaxExceptions`: list of syntax exceptions encountered during the analysis. These exceptions are due to rules or regular expressions of the grammar not being respected in the source file.
- `EndOfFileStatus`: status of the analyse once the end of file is reached.

## Exceptions
The syntax analyser can throw other types of exceptions that are listed into this array

|Exception|Description|
|--|--|
|RuleNotFoundException|A symbol to derive was reached by the analyser, but no rules nor regular expressions have been found to derive it.|
|AmbiguousGrammar|The grammar has some ambiguities and cannot be parsed. No more description can be provided.|

In addition to that, some exceptions can also be thrown while parsing the grammar to make it LL(1) compliant.

|Exception|Description|Remarks|
|--|--|--|
|EmptyRegExPatternException|The regular expression pattern is empty and cannot be merged.|You may have an empty regex pattern in your grammar file.|
|EmptyRegExNameException|The regular expression symbol is empty and cannot be merged.|This error should not occur to you. Please report it if it does.|
|EmptyPatternException, PatternNotFoundException, InvalidRulesException|Something wrong happened with factorising the grammar.|Please report any of these issues.|
