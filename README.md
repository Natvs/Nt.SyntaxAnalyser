# Syntax Analyser

### Introduction
This project is following the [SyntaxParser project](https://github.com/Natvs/Nt.SyntaxParser). While the syntax parser aims to read a grammar from a string or file and create a grammar structure, this one aims to produce a syntax tree given a grammar and a source code file.

In order to do so, it reuses the parser present in the parser project that tokenizes the source code file, and then applies a descending parser algorithm in order to produce the syntax tree. For that analysis, the grammar must be LL(1). This project includes some methods like factorisation and derecursivation to make it LL(1) compliant. However, as it is not guaranteed that the grammar will be LL(1) after applying these methods, further manual restructuration of the grammar might be required.

### Pretreating the grammar
In order to make the grammar LL(1) compliant, two pretreatments are applied to the grammar before using it to produce a syntax tree: factorisation and derecursivation.

#### Factorisation
Factorisation consists in extracting common prefixes from the productions of a same non-terminal. For example, given the following rules 

`A -> a B | a C | d`

the resulting rules would be 

`A -> a A' | d` and `A' -> B | C`.

#### Derecursivation
Derecursivation consists in removing left recursion from the grammar. These recursions can be direct or indirect. For example, given the following rules

`A -> A a | b`

the resulting rules would be

`A -> b A'` and `A' -> a A' | `.

> Note: a rule like `A' -> ` means that A' produces the empty string.

### Using the Syntax Analyser
In order to use the syntax analyser, you need to provide a grammar and a source code file. The syntax of a grammar file is described in the [documentation of the SyntaxParser project](https://github.com/Natvs/Nt.SyntaxParser/blob/master/Doc/Grammar.md). The source code file must be written in accordance with the provided grammar.