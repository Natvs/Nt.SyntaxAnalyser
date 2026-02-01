module Nt.Syntax.LLAnalysing.LL1Analyser

open System.Text.RegularExpressions

open Nt.Parser
open Nt.Parser.Symbols
open Nt.Parser.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.LL1AnalyseSet
open Nt.Syntax.LLAnalysing.Utils

// Exceptions handled by the analyser
exception public SyntaxException of ParsedToken
exception public UnexpectedEndOfFileException

// Exceptions the analyser can trigger
exception public AmbiguousGrammar
exception public RuleNotFoundException of ISymbol
exception public UnknownSymbolType of GrammarToken

/// Context used during the analysis process
type private AnalyserContext = {
    Terminals: ISymbol list
    NonTerminals: ISymbol list
    EOF: ISymbol
    Rules: EnrichedRule list
    Regexs: EnrichedRegex list
    Parsed : ParsedToken list
    ErrorCheckpoint : ISymbol list
    mutable CurrentIndex : int
    mutable SyntaxError: bool
    mutable SyntaxExceptionsList : SyntaxException list
}

/// Final status of the end of file analysis
type public EndOfFileStatus =
    | Valid = 0
    | Failed = 1

/// Result of the analysis process
type public AnalyseResult = {
    SyntaxExceptions: SyntaxException list
    EndOfFileStatus: EndOfFileStatus
}

/// Check if the current parsed token is a checkpoint
let private is_current_check_point (context: AnalyserContext) =
    let symbol = context.Parsed[context.CurrentIndex].Symbol
    context.ErrorCheckpoint 
    |> List.contains symbol

/// Check if a grammar token is a checkpoint
let private is_check_point (context: AnalyserContext) (token: GrammarToken) =
    context.ErrorCheckpoint 
    |> List.contains token.Symbol

/// Move to the next parsed token
let private read (context: AnalyserContext) =
    context.CurrentIndex <- context.CurrentIndex + 1
    match context.CurrentIndex >= context.Parsed.Length with
    | true -> raise UnexpectedEndOfFileException
    | false -> context

/// Move to the next checkpoint in the parsed tokens
let rec private next_check_point(context: AnalyserContext) =
    match context |> is_current_check_point with
    | true -> context
    | false -> context |> read |> next_check_point

/// Handle a syntax exception by recording it and moving to the next checkpoint
let private handle_syntax_exception (ex: SyntaxException) (context: AnalyserContext) =
    context.SyntaxExceptionsList <- context.SyntaxExceptionsList@[ex]
    context.SyntaxError <- true
    context |> next_check_point

/// Reset the syntax exception flag in the context
let private reset_syntax_exception (context : AnalyserContext) =
    context.SyntaxError <- false
    context

/// Handle a terminal token in the parsing process
let rec private handle_terminal (term: Terminal) (context: AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex]
    if context.SyntaxError
    then
        if term |> is_check_point context 
        then context |> reset_syntax_exception |> handle_terminal term
        else context
    else
        try
            match parsed.Symbol with
            | s when s <> term.Symbol -> 
                    raise (SyntaxException(parsed))
            | _ -> ()
            if (context.CurrentIndex < context.Parsed.Length-1)
            then context |> read 
            else context
        with
        | :? SyntaxException as ex -> context |> handle_syntax_exception ex

/// Handle a regular expression token in the parsing process
let private handle_regex (symbol: ISymbol) (context: AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex]
    if context.SyntaxError
    then 
        context
    else
        try 
            if context.Regexs |> List.exists (fun rg ->
                rg.Token.Symbol = symbol &&
                let regex = Regex(rg.Pattern) in regex.IsMatch(parsed.Symbol.Name)
            ) = false then
                raise (SyntaxException parsed)       
            if (context.CurrentIndex < context.Parsed.Length - 1)
            then context |> read
            else context
        with
        | :? SyntaxException as ex -> context |> handle_syntax_exception ex
     
/// Handle a sequence of grammar tokens in the parsing process
let rec private handle_sequence (sequence: GrammarToken list) (context: AnalyserContext) =
    match sequence with
    | [] -> context
    | (:? Terminal as term)::tail -> 
        context
        |> handle_terminal term
        |> handle_sequence tail
    | (:? NonTerminal as nonterm)::tail ->
        context
        |> handle_non_terminal nonterm.Symbol
        |> handle_sequence tail
    | symbol::_ -> raise (UnknownSymbolType symbol)

/// Handle a non-terminal token in the parsing process
and private handle_non_terminal (symbol :ISymbol) (context : AnalyserContext) =
    let is_terminal (s: ISymbol) (terminals: ISymbol list) =
        terminals
        |> List.map (fun t -> t.Name)
        |> List.contains s.Name

    let parsed = context.Parsed[context.CurrentIndex]
    if context.Rules |> List.exists (fun r -> r.Token.Symbol = symbol) then
        // Handles the case where a rule exists
        if  is_terminal parsed.Symbol context.Terminals = false
        then
            let rule = context.Rules |> List.find(fun r -> r.Token.Symbol = symbol)
            context |> handle_sequence rule.Derivation
        else
            let rules =
                context.Rules
                |> List.filter (fun r -> r.Token.Symbol = symbol && r.DirectiveSymbols |> Set.contains (parsed.Symbol :?> SyntaxSymbol))

            if rules.Length = 0 then
                context |> handle_regex symbol
            elif rules.Length = 1 then
                let rule = rules.Head
                context |> handle_sequence rule.Derivation
            else
                raise AmbiguousGrammar
    elif context.Regexs |> List.exists (fun rg -> rg.Token.Symbol = symbol) then
        // Handles the case where there are no rules but a regular expression
        context |> handle_regex symbol
    else
        raise (RuleNotFoundException symbol)

/// Analyse a parser result against an analyse set
[<CompiledName("Analyse")>]
let public analyse (analyseSet: AnalyseSet) (parserResult: ParserResult) =
    let context = { 
        Terminals = analyseSet.Terminals |> List.ofSeq
        NonTerminals = analyseSet.NonTerminals |> List.ofSeq
        EOF = analyseSet.EOF
        Rules = analyseSet.Rules |> List.ofSeq
        Regexs = analyseSet.RegEx |> List.ofSeq
        Parsed = (parserResult.GetParsed() |> List.ofSeq)@[new ParsedToken(analyseSet.EOF, 0)]
        ErrorCheckpoint = analyseSet.Checkpoints |> List.ofSeq
        CurrentIndex = 0
        SyntaxError = false
        SyntaxExceptionsList = []
    }

    let mutable endOfFileStatus = EndOfFileStatus.Valid
    try
        context 
        |> handle_sequence ( new NonTerminal(analyseSet.Axiom, -1)::new Terminal(analyseSet.EOF, -1)::[] )
        |> ignore
    with
    | :? UnexpectedEndOfFileException -> endOfFileStatus <- EndOfFileStatus.Failed

    {
        SyntaxExceptions = context.SyntaxExceptionsList
        EndOfFileStatus = endOfFileStatus
    }
