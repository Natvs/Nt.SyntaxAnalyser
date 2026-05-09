namespace Nt.Syntax

open System.IO

open Nt.Syntax.Structures;
open Nt.Parser
open Nt.Syntax.LLAnalysing.LL1Analyser
open Nt.Syntax.LLAnalysing.LL1AnalyseSet
open Nt.Syntax.LLParsing.LL1Parser;
open Nt.Syntax.LLAnalysing.Utils

type SourceReader (
    grammar: Grammar,
    separators: System.Collections.Generic.List<char>, 
    symbols: System.Collections.Generic.List<string>,
    checkpoints: System.Collections.Generic.List<char>
    ) =

    let _analyse_set = 
        get_lookahead_set 
            (grammar |> parse_to_LL1) 
            checkpoints

    let _reader = SymbolsParser(
        SyntaxSymbolFactory(),
        separators,
        symbols
    )

    // Analyse a string
    member this.AnalyseString(content: string) =
        _reader.Parse(content)
        |> analyse_from_analyse_set _analyse_set
        

    // Analyse the content of a file
    member this.AnalyseFile(filename: string) = 
        let content = File.ReadAllText(filename)
        this.AnalyseString(content)

    static member CreateFromGrammar (grammar: Grammar, separators: System.Collections.Generic.List<char>, symbols: System.Collections.Generic.List<string>, checkpoints: System.Collections.Generic.List<char>) =
        SourceReader(grammar, separators, symbols, checkpoints)

    static member CreateFromString (content: string, separators: System.Collections.Generic.List<char>, symbols: System.Collections.Generic.List<string>, checkpoints: System.Collections.Generic.List<char>) =
        SourceReader(parse_grammar_from_string content, separators, symbols, checkpoints)

    static member CreateFromFile (filename: string, separators: System.Collections.Generic.List<char>, symbols: System.Collections.Generic.List<string>, checkpoints: System.Collections.Generic.List<char>) =
        SourceReader(parse_grammar_from_file filename, separators, symbols, checkpoints)