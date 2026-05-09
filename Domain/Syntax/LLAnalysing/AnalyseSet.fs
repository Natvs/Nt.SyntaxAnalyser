module Nt.Syntax.LLAnalysing.LL1AnalyseSet

open Nt.Parser.Structures
open Nt.Parser.Symbols
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing
open Nt.Syntax.LLAnalysing.Utils

/// Structure containing a grammar rule enriched with directive symbols
type public EnrichedRule = {
    Token : NonTerminal
    Derivation : GrammarToken list
    DirectiveSymbols : Set<SyntaxSymbol>
}

/// Structure containing a regex pattern enriched with start and end anchors
type public EnrichedRegex = {
    Token : NonTerminal
    Pattern : string
}

/// Structure containing the lookahead set of a grammar
type public AnalyseSet = {
    Axiom: ISymbol
    EOF: ISymbol
    Terminals: ISymbol list
    NonTerminals: ISymbol list
    Checkpoints: ISymbol list
    Rules: EnrichedRule list
    RegEx: EnrichedRegex list
}

/// Get a structure containing rule information enriched with directive symbols
let get_enriched_rule (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (follows: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (rule: Rule) =
    if (empty_generators |> List.contains rule.Token.Symbol) 
        then 
            let token_follows = 
                follows.TryFind(rule.Token.Symbol :?> SyntaxSymbol)
                |> Option.defaultValue Set.empty
            let directive_symbols =
                rule.Derivation 
                |> List.ofSeq
                |> get_sequence_firsts empty_generators firsts
                |> Set.union token_follows
            { Token = rule.Token; Derivation = rule.Derivation |> List.ofSeq; DirectiveSymbols = directive_symbols }
        else 
            let directive_symbols =
                rule.Derivation
                |> List.ofSeq
                |> get_sequence_firsts empty_generators firsts
            { Token = rule.Token; Derivation = rule.Derivation |> List.ofSeq; DirectiveSymbols = directive_symbols }

/// Get a pattern enriched with start and end anchors
let rec get_sub_enriched_regex (pattern: char list) (depth: int) =
    match depth, pattern with
    | _, [] -> []
    | 0, '|'::tail -> '$'::'|'::'^'::(get_sub_enriched_regex tail depth)
    | _, '('::tail -> '('::(get_sub_enriched_regex tail (depth + 1))
    | _, ')'::tail -> ')'::(get_sub_enriched_regex tail (depth - 1))
    | _, c::tail -> c::(get_sub_enriched_regex tail depth) 

/// Get a regex structure enriched with start and end anchors
let get_enriched_regex (regex: RegularExpression) =
    let new_pattern =
        get_sub_enriched_regex (regex.Pattern |> List.ofSeq) 0
        |> List.toArray
        |> System.String
    { Token = regex.Token; Pattern = "^" + new_pattern + "$" }

/// Get all enriched rules from a list of rules
let rec private get_all_enriched_rules (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (follows: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (rules: Rule list) =
    match rules with
    | [] -> []
    | rule::tail -> (rule |> get_enriched_rule empty_generators firsts follows)::(tail |> get_all_enriched_rules empty_generators firsts follows)

/// Get all enriched regexs from a list of regexs
let rec private get_all_enriched_regexs (regexs: RegularExpression list) =
    match regexs with
    | [] -> []
    | rg::tail -> (rg |> get_enriched_regex)::(tail |> get_all_enriched_regexs)

/// Public function to get the lookahead set of a grammar
[<CompiledName("Get")>]
let public get_lookahead_set (g: Grammar) (checkpoints: System.Collections.Generic.List<char>) =
    let empty_generators = g |> EmptyAnalyser.analyse
    let firsts = (g, empty_generators) ||> FirstsAnalyser.analyse
    let follows = (g, empty_generators, firsts) |||> FollowsAnalyser.analyse
    let eof = g.Terminals.Get("EOF");
    {
        Axiom = g.Axiom.Symbol
        EOF = eof
        Terminals = g.Terminals.GetSymbols() |> List.ofSeq
        NonTerminals = g.NonTerminals.GetSymbols() |> List.ofSeq
        Checkpoints = checkpoints |> List.ofSeq |> List.map (fun name -> Symbol(sprintf "%c" name))
        Rules = g.Rules |> List.ofSeq |> get_all_enriched_rules empty_generators firsts follows;
        RegEx = g.RegularExpressions |> List.ofSeq |> get_all_enriched_regexs
    }