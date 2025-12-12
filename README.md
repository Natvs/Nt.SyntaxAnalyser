# Syntax Analyser

### Introduction
This project is following the [SyntaxParser project](https://github.com/Natvs/Nt.SyntaxParser). While the syntax parser aims to read a grammar from a string or file and create a grammar structure, this one aims to produce a syntax tree given a grammar and a source code file.

In order to do so, it reuses the parser present in the parser project that tokenizes the source code file, and then applies a descending parser algorithm in order to produce the syntax tree. For that analysis, the grammar must be LL(1). This project includes some methods like factorisation and derecursivation to make it LL(1) compliant. However, as it is not guaranteed that the grammar will be LL(1) after applying these methods, further manual restructuration of the grammar might be required.

### Using the Syntax Analyser
In order to use the syntax analyser, you need to provide a grammar and a source code file. The syntax of a grammar file is described in the [documentation of the SyntaxParser project](https://github.com/Natvs/Nt.SyntaxParser/blob/master/Doc/Grammar.md). The source code file must be written in accordance with the provided grammar.

#### Set up the grammar

Once your grammar file is set, you need to create a `Grammar` instance with
```csharp
using Nt.Syntax;
using Nt.Syntax.Structures;

var syntaxparser = new SyntaxParser();
Grammar grammar = parser.ParseFile("path/to/grammar/file");
```

In order to make the grammar usable by the `LL1Analyser`, you first have to make it LL(1) with
```csharp
using Nt.Syntax.LLParsing;

LL1Parser.Parse(grammar);
```
For more information about pre-treatment, refer to [the documentation for this step](Docs/Pretreatment.md). This step is not needed if your grammar is already LL(1).

#### Prepare for analysing your source file

Once your grammar is ready, you need to read your source file.
```csharp
using System.IO;
using Nt.Parsing;

var input = File.ReadAllText("path/to/the/source/file");
Parser parser = new Parser([' ', '\n'], ["{", "}", ";", "\""]);
ParserResult parserResult = parser.Parse(input);
```
Please refer to [the parser documentation](https://github.com/Natvs/Nt.SyntaxParser/blob/master/Readme.md) for information about the parsing arguments.

#### Analyse your source file according to your grammar

The signature of the analyse method is
```csharp
LLAnalyser.AnalyseResult Analyse(Grammar grammar, ParserResult parserResult, SymbolsList checkpoint)
```
where
- `grammar` is the grammar structure computed in the [step for setting up the grammar](#set-up-the-grammar).
- `parserResult` is structure that contains the parsing results of the source file computed in the [previous step](#prepare-for-analysing-your-source-file)
- `SymbolsList` is a list of symbols to reach in case of errors, so that the analysing can continue. By instance, for a programming language with C syntax, these are `;` for the end of an expression and `}` for the end of a block.

For the previously computed `grammar` and `parserResult`, you can call the analyse method like this:
```csharp
using Nt.Syntax.LLAnalysing;

var checkpoints = new SymbolsList([";"]);
LL1Analyser.AnalyseResult = LL1Analyser.Analyse(grammar, parserResult, checkpoints);
```

The method return an `AnalyseResult` structure that contains datas about the analysis, including the list of errors encountered during the analysis.

The fields of such structure are:
- `SyntaxExceptions`: list of syntax exceptions encountered during the analysis. These exceptions are due to rules of the grammar not being respected in the source file, and does not contain exceptions involving regular expressions mismatches.
- `RegexExceptions`: list of regular expression exceptions encountered during the analysis.
- `EndOfFileStatus`: status of the analyse once the end of file is reached.