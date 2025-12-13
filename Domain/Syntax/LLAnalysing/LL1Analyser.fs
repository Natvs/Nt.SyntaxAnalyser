module Nt.Syntax.LLAnalysing.LL1Analyser

open System.Text.RegularExpressions
open Nt.Parsing
open Nt.Parsing.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.LL1AnalyseSet

exception public SyntaxException of ParsedToken
exception public RegexException of ParsedToken * string
exception public UnknownSymbolType of GrammarToken
exception public UnexpectedEndOfFileException

type private AnalyserContext = {
    AnalyseSet : AnalyseSet
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

let private isCheckPoint (context: AnalyserContext) (token: GrammarToken) =
    let tokenString = context.AnalyseSet.Terminals[token.Index].Name
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

let private isRegExSymbol (token: GrammarToken) (context: AnalyserContext) =
    context.AnalyseSet.RegEx 
    |> List.exists (fun e -> e.Token.Index = token.Index)

let private handleTerminal (term: Terminal) (context: AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex];
    match context.SyntaxError, context.Symbols[parsed.TokenIndex].Name with
    | true, _ when term |> isCheckPoint context -> context |> resetSyntaxException |> ignore
    | _, s when s <> context.AnalyseSet.Terminals[term.Index].Name -> 
        try
            raise (SyntaxException(parsed))
        with
            | :? SyntaxException as ex -> context |> handleSyntaxException ex |> ignore
    | _ -> ()
    if (context.CurrentIndex < context.Parsed.Length-1) then context |> read
    else context

let private handleRegex (nonterm: NonTerminal) (context: AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex];
    let regex =
        context.AnalyseSet.RegEx
        |> List.find(fun e -> e.Token.Index = nonterm.Index)
    match context.SyntaxError, context.Symbols[parsed.TokenIndex].Name with
    | false, s when (s, regex.Pattern) |> Regex.IsMatch = false -> raise (RegexException (parsed, regex.Pattern))
    | _ -> ()
    context |> read

let rec private compute_sequence (sequence: GrammarToken list) (context: AnalyserContext) =
    match sequence with
    | [] -> ()
    | (:? Terminal as term)::tail -> 
        context
        |> handleTerminal term
        |> compute_sequence tail
    | (:? NonTerminal as nonterm)::tail ->
        context
        |> handle_non_terminal nonterm.Index
        |> compute_sequence tail
    | symbol::_ -> raise (UnknownSymbolType symbol)

and private handle_non_terminal (index :int) (context : AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex]
    if (context.AnalyseSet.Terminals |> List.exists (fun t -> context.Symbols[parsed.TokenIndex].Name = t.Name))
        then
        let terminal_index =
            context.AnalyseSet.Terminals
            |> List.findIndex (fun s -> context.Symbols[parsed.TokenIndex].Name = s.Name)
        context.AnalyseSet.Rules
        |> List.filter (fun r -> r.Rule.Token.Index = index && r.Symbols |> List.contains terminal_index)
        |> List.iter (fun r-> 
            context
            |> compute_sequence (r.Rule.Derivation |> List.ofSeq)
        )
    else 
        try
            raise (SyntaxException(parsed))
        with
        | :? SyntaxException as ex -> context |> handleSyntaxException ex |> ignore
    context
    (*|> List.iter (fun r ->   
        
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
    )*)

[<CompiledName("Analyse")>]
let public analyse (analyseSet: AnalyseSet) (parserResult: ParserResult) (checkpoints: SymbolsList) =
    let new_index = parserResult.Symbols.Add(analyseSet.EOF.Name)
    let context = { 
        AnalyseSet = analyseSet
        Symbols = (parserResult.Symbols |> List.ofSeq)
        Parsed = (parserResult.Parsed |> List.ofSeq)@[new ParsedToken(new_index, 0)]
        ErrorCheckpoint = checkpoints |> List.ofSeq
        CurrentIndex = 0
        SyntaxError = false
        SyntaxExceptionsList = []
        RegexExceptionsList = []
    }

    let mutable endOfFileStatus = EndOfFileStatus.Valid
    try
        context |> compute_sequence ((new NonTerminal(analyseSet.Axiom, -1))::(new Terminal(analyseSet.EOF.Index, -1))::[]) |> ignore
    with
    | :? UnexpectedEndOfFileException as ex -> endOfFileStatus <- EndOfFileStatus.Failed

(*    match context.Parsed.Count with
    | i when i = context.CurrentIndex -> ()
    | _ -> endOfFileStatus <- Failed *)

    {
        SyntaxExceptions = context.SyntaxExceptionsList
        RegexExceptions = context.RegexExceptionsList
        EndOfFileStatus = endOfFileStatus
    }
