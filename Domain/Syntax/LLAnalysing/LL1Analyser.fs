module Nt.Syntax.LLAnalysing.LL1Analyser

open System.Text.RegularExpressions
open Nt.Parsing
open Nt.Parsing.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.LL1AnalyseSet

exception public SyntaxException of ParsedToken * Terminal
exception public RegexException of ParsedToken * string
exception public UnknownSymbolType of GrammarToken
exception public UnexpectedEndOfFileException

type private AnalyserContext = {
    Datas : AnalyseSet
    Symbols : Symbol list
    Parsed : ParsedToken list
    ErrorCheckpoint : Symbol list
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
    |> List.map (fun cp -> cp.Name)
    |> List.contains tokenString

let private isCheckPoint (context: AnalyserContext) (token: Terminal) =
    let tokenString = context.Datas.Terminals[token.Index].Name
    context.ErrorCheckpoint 
    |> List.map (fun cp -> cp.Name)
    |> List.contains tokenString

let private read (context: AnalyserContext) =
    context.CurrentIndex <- context.CurrentIndex + 1
    match context.CurrentIndex >= context.Parsed.Length with
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

let private isRuleMatching (parsed: ParsedToken) (r: EnrichedRule) (context: AnalyserContext) =
    let firstSymbolIndex = r.Rule.Derivation[0].Index
    let currentToken = context.Symbols[parsed.TokenIndex]
    r.Rule.Derivation.Count > 0 &&
    (
        r.Rule.Derivation[0].Type = GrammarTokenType.Terminal && context.Datas.Terminals[firstSymbolIndex].Name = currentToken.Name
        ||
        r.Rule.Derivation[0].Type = GrammarTokenType.NonTerminal && context.Datas.NonTerminals[firstSymbolIndex].Name = currentToken.Name
    )

let private isRegExSymbol (token: GrammarToken) (context: AnalyserContext) =
    context.Datas.RegEx 
    |> List.exists (fun e -> e.Token.Index = token.Index)

let private handleTerminal (parsed: ParsedToken) (term: Terminal) (context: AnalyserContext) =
    match context.SyntaxError, context.Symbols[parsed.TokenIndex].Name with
    | true, _ when term |> isCheckPoint context -> context |> resetSyntaxException |> ignore
    | _, s when s <> context.Datas.Terminals[term.Index].Name -> raise (SyntaxException (parsed, term))
    | _ -> ()
    if (context.CurrentIndex+1 < context.Parsed.Length) then context |> read
    else context

let private handleRegex (parsed: ParsedToken) (nonterm: NonTerminal) (context: AnalyserContext) =
    let regex =
        context.Datas.RegEx
        |> List.find(fun e -> e.Token.Index = nonterm.Index)
    match context.SyntaxError, context.Symbols[parsed.TokenIndex].Name with
    | false, s when (s, regex.Pattern) |> Regex.IsMatch = false -> raise (RegexException (parsed, regex.Pattern))
    | _ -> ()
    context |> read

let rec private analyseRules (context : AnalyserContext) (symbol_rules : EnrichedRule list) =
    let token = context.Parsed[context.CurrentIndex]
    symbol_rules
    |> List.iter (fun r ->        
        match (r, context) ||> isRuleMatching token with
        | false ->
            try
                raise (SyntaxException(token, new Terminal(0, 0)))
            with
            | :? SyntaxException as ex -> context |> handleSyntaxException ex |> ignore
        | true ->
            r.Rule.Derivation
            |> List.ofSeq
            |> List.iter (fun symbol -> 
                try
                    match symbol with
                    | :? NonTerminal as nonterm when context |> isRegExSymbol symbol -> context |>  handleRegex token nonterm |> ignore         
                    | :? NonTerminal -> analyseRules context (context.Datas.Rules |> List.ofSeq |> List.filter (fun rr -> rr.Rule.Token.Index = symbol.Index ))
                    | :? Terminal as term -> context |> handleTerminal token term |> ignore
                    | _ -> raise (UnknownSymbolType symbol)
                with
                | :? SyntaxException as ex -> context |> handleSyntaxException ex |> ignore
                | :? RegexException as ex -> context |> handleRegexException ex |> ignore
                | _ as ex -> reraise()
            )
    )

[<CompiledName("Analyse")>]
let public analyse (lookahead: AnalyseSet) (parserResult: ParserResult) (checkpoints: SymbolsList) =
    let context = { 
        Datas = lookahead
        Symbols = parserResult.Symbols |> List.ofSeq
        Parsed = parserResult.Parsed |> List.ofSeq
        ErrorCheckpoint = checkpoints |> List.ofSeq
        CurrentIndex = 0
        SyntaxError = false
        SyntaxExceptionsList = []
        RegexExceptionsList = []
    }
    let rules = lookahead.Rules |> List.ofSeq |> List.filter (fun rr -> rr.Rule.Token.Index = lookahead.Axiom )

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
