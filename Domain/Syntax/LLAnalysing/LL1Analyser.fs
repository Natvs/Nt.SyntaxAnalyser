module Nt.Syntax.LLAnalysing.LL1Analyser

open Nt.Syntax.Structures
open Nt.Parsing.Structures
open Nt.Parsing
open System.Text.RegularExpressions

exception public SyntaxException of ParsedToken * Terminal
exception public RegexException of ParsedToken * string
exception public UnknownSymbolType of GrammarToken
exception public UnexpectedEndOfFileException

type private AnalyserContext = {
    Grammar : Grammar
    Symbols : SymbolsList
    Parsed : ParsedList
    ErrorCheckpoint : SymbolsList
    mutable CurrentIndex : int
    mutable SyntaxError: bool
    mutable SyntaxExceptionsList : SyntaxException list
    mutable RegexExceptionsList : RegexException list
}

type public EndOfFileStatus =
    | Valid = 0
    | Failed = 1

type public AnalyseResult = {
    SyntaxExceptions: SyntaxException list
    RegexExceptions: RegexException list
    EndOfFileStatus: EndOfFileStatus
}

let private isCurrentCheckPoint (context: AnalyserContext) =
    let tokenString = context.Symbols[context.Parsed[context.CurrentIndex].TokenIndex].Name
    context.ErrorCheckpoint 
    |> List.ofSeq
    |> List.map (fun cp -> cp.Name)
    |> List.contains tokenString

let private isCheckPoint (context: AnalyserContext) (token: Terminal) =
    let tokenString = context.Grammar.Terminals[token.Index].Name
    context.ErrorCheckpoint 
    |> List.ofSeq
    |> List.map (fun cp -> cp.Name)
    |> List.contains tokenString

let private read (context: AnalyserContext) =
    context.CurrentIndex <- context.CurrentIndex + 1
    match context.CurrentIndex >= context.Parsed.Count with
    | true -> raise UnexpectedEndOfFileException
    | false -> context

let rec private goToNextCheckpoint(context: AnalyserContext) =
    match context |> isCurrentCheckPoint with
    | true -> context
    | false -> context |> read |> goToNextCheckpoint

let private handleSyntaxException (ex: SyntaxException) (context: AnalyserContext) =
    context.SyntaxExceptionsList <- ex::context.SyntaxExceptionsList
    context.SyntaxError <- true
    context |> goToNextCheckpoint

let private handleRegexException (ex: RegexException) (context: AnalyserContext) =
    context.RegexExceptionsList <- ex::context.RegexExceptionsList
    context.SyntaxError <- true
    context |> goToNextCheckpoint

let private resetSyntaxException (context : AnalyserContext) =
    context.SyntaxError <- false
    context

let private isRuleMatching (parsed: ParsedToken) (r: Rule) (context: AnalyserContext) =
    let firstSymbolIndex = r.Derivation[0].Index
    let currentToken = context.Symbols[parsed.TokenIndex]
    r.Derivation.Count > 0 &&
    (
        r.Derivation[0].Type = GrammarTokenType.Terminal && context.Grammar.Terminals[firstSymbolIndex].Name = currentToken.Name
        ||
        r.Derivation[0].Type = GrammarTokenType.NonTerminal && context.Grammar.NonTerminals[firstSymbolIndex].Name = currentToken.Name
    )

let private isRegExSymbol (token: GrammarToken) (context: AnalyserContext) =
    context.Grammar.RegExSymbols 
    |> List.ofSeq 
    |> List.map(fun (e: Symbol) -> e.Name) 
    |> List.contains context.Grammar.NonTerminals[token.Index].Name

let private handleTerminal (parsed: ParsedToken) (term: Terminal) (context: AnalyserContext) =
    match context.SyntaxError, context.Symbols[parsed.TokenIndex].Name with
    | true, _ when term |> isCheckPoint context -> context |> resetSyntaxException |> ignore
    | _, s when s <> context.Grammar.Terminals[term.Index].Name -> raise (SyntaxException (parsed, term))
    | _ -> ()
    if (context.CurrentIndex+1 < context.Parsed.Count) then context |> read
    else context

let private handleRegex (parsed: ParsedToken) (nonterm: NonTerminal) (context: AnalyserContext) =
    let regex =
        context.Grammar.RegularExpressions
        |> List.ofSeq
        |> List.find(fun e -> e.Token.Index = nonterm.Index)
    match context.SyntaxError, context.Symbols[parsed.TokenIndex].Name with
    | false, s when (s, regex.Pattern) |> Regex.IsMatch = false -> raise (RegexException (parsed, regex.Pattern))
    | _ -> ()
    context |> read

let rec private analyseRules (context : AnalyserContext) (rules : Rule list) =
    let token = context.Parsed[context.CurrentIndex]
    rules
    |> List.iter (fun r ->        
        match (r, context) ||> isRuleMatching token with
        | false ->
            try
                raise (SyntaxException(token, new Terminal(0, 0)))
            with
            | :? SyntaxException as ex -> context |> handleSyntaxException ex |> ignore
        | true ->
            r.Derivation
            |> List.ofSeq
            |> List.iter (fun symbol -> 
                try
                    match symbol with
                    | :? NonTerminal as nonterm when context |> isRegExSymbol symbol -> context |>  handleRegex token nonterm |> ignore         
                    | :? NonTerminal -> analyseRules context (context.Grammar.Rules |> List.ofSeq |> List.filter (fun rr -> rr.Token.Index = symbol.Index ))
                    | :? Terminal as term -> context |> handleTerminal token term |> ignore
                    | _ -> raise (UnknownSymbolType symbol)
                with
                | :? SyntaxException as ex -> context |> handleSyntaxException ex |> ignore
                | :? RegexException as ex -> context |> handleRegexException ex |> ignore
                | _ as ex -> reraise()
            )
    )

[<CompiledName("Analyse")>]
let public analyse (grammar: Grammar) (parserResult: ParserResult) (checkpoints: SymbolsList) =
    let context = { 
        Grammar = grammar
        Symbols = parserResult.Symbols
        Parsed = parserResult.Parsed
        ErrorCheckpoint = checkpoints
        CurrentIndex = 0
        SyntaxError = false
        SyntaxExceptionsList = []
        RegexExceptionsList = []
    }
    let rules = grammar.Rules |> List.ofSeq |> List.filter (fun rr -> rr.Token.Index = grammar.Axiom )

    let mutable endOfFileStatus = EndOfFileStatus.Valid
    try
        (context, rules) ||> analyseRules
    with
    | :? UnexpectedEndOfFileException as ex -> endOfFileStatus <- EndOfFileStatus.Failed

(*    match context.Parsed.Count with
    | i when i = context.CurrentIndex -> ()
    | _ -> endOfFileStatus <- Failed*)

    {
        SyntaxExceptions = context.SyntaxExceptionsList
        RegexExceptions = context.RegexExceptionsList
        EndOfFileStatus = endOfFileStatus
    }
