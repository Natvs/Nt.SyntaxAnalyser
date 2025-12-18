module Nt.Syntax.LLAnalysing.LL1Analyser

open System.Text.RegularExpressions
open System.Collections.Generic
open Nt.Parsing
open Nt.Parsing.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.LL1AnalyseSet

// Exceptions handled by the analyser
exception public SyntaxException of ParsedToken
exception public UnexpectedEndOfFileException

// Exceptions the analyser can trigger
exception public AmbiguousGrammar
exception public RuleNotFound of Symbol
exception public UnknownSymbolType of GrammarToken

type private AnalyserContext = {
    AnalyseSet : AnalyseSet
    Symbols : Symbol list
    Parsed : ParsedToken list
    ErrorCheckpoint : Symbol list
    mutable CurrentIndex : int
    mutable SyntaxError: bool
    mutable SyntaxExceptionsList : SyntaxException list
}

type public EndOfFileStatus =
    | Valid = 0
    | Failed = 1

type public AnalyseResult = {
    SyntaxExceptions: SyntaxException list
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
    context.SyntaxExceptionsList <- context.SyntaxExceptionsList@[ex]
    context.SyntaxError <- true
    context |> goToNextCheckpoint

let private resetSyntaxException (context : AnalyserContext) =
    context.SyntaxError <- false
    context

let rec private handleTerminal (term: Terminal) (context: AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex]
    if context.SyntaxError
    then
        if term |> isCheckPoint context 
        then context |> resetSyntaxException |> handleTerminal term
        else context
    else
        try
            match context.Symbols[parsed.TokenIndex].Name with
            | s when s <> context.AnalyseSet.Terminals[term.Index].Name -> 
                    raise (SyntaxException(parsed))
            | _ -> ()
            if (context.CurrentIndex < context.Parsed.Length-1)
            then context |> read 
            else context
        with
        | :? SyntaxException as ex -> context |> handleSyntaxException ex

let private handleRegex (index: int) (context: AnalyserContext) =
    let parsed = context.Parsed[context.CurrentIndex]
    if context.SyntaxError
    then 
        context
    else
        try 
            if context.AnalyseSet.RegEx |> List.exists (fun rg ->
                rg.Token.Index = index &&
                let regex = Regex(rg.Pattern) in regex.IsMatch(context.Symbols[parsed.TokenIndex].Name)
            ) = false then
                raise (SyntaxException parsed)       
            if (context.CurrentIndex < context.Parsed.Length - 1)
            then context |> read
            else context
        with
        | :? SyntaxException as ex -> context |> handleSyntaxException ex
        
let rec private compute_sequence (sequence: GrammarToken list) (context: AnalyserContext) =
    match sequence with
    | [] -> context
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
    if context.AnalyseSet.Rules |> List.exists (fun r -> r.Token.Index = index) then
        // Handles the case where a rule exists
        if context.AnalyseSet.Terminals |> List.exists (fun s -> context.Symbols[parsed.TokenIndex].Name = s.Name) = false
        then
            let rule = context.AnalyseSet.Rules |> List.find(fun r -> r.Token.Index = index)
            context |> compute_sequence rule.Derivation
        else
            let terminal_index =
                context.AnalyseSet.Terminals
                |> List.findIndex (fun s -> context.Symbols[parsed.TokenIndex].Name = s.Name)

            let rules =
                context.AnalyseSet.Rules
                |> List.filter (fun r -> r.Token.Index = index && r.DirectiveSymbols |> List.contains terminal_index)

            if rules.Length = 0 then
                context |> handleRegex index
            elif rules.Length = 1 then
                let rule = rules.Head
                context |> compute_sequence rule.Derivation
            else
                raise AmbiguousGrammar
    elif context.AnalyseSet.RegEx |> List.exists (fun rg -> rg.Token.Index = index) then
        // Handles the case where there are no rules but a regular expression
        context |> handleRegex index
    else
        raise (RuleNotFound context.AnalyseSet.NonTerminals[index])

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
    }

    let mutable endOfFileStatus = EndOfFileStatus.Valid
    try
        context |> compute_sequence ((new NonTerminal(analyseSet.Axiom, -1))::(new Terminal(analyseSet.EOF.Index, -1))::[]) |> ignore
    with
    | :? UnexpectedEndOfFileException as ex -> endOfFileStatus <- EndOfFileStatus.Failed

    {
        SyntaxExceptions = context.SyntaxExceptionsList
        EndOfFileStatus = endOfFileStatus
    }
