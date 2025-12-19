module Nt.Syntax.LLAnalysing.LL1AnalyseSet

open Nt.Parsing.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing
open Nt.Syntax.LLAnalysing.Utils

type EnrichedRule = {
    Token : NonTerminal
    Derivation : GrammarToken list
    DirectiveSymbols : int list
}

type EnrichedRegex = {
    Token : NonTerminal
    Pattern : string
}

type EOF_Type = {
    Name: string
    Index: int
}

type AnalyseSet = {
    Axiom: int
    EOF: EOF_Type
    Terminals: Symbol list
    NonTerminals: Symbol list
    Rules: EnrichedRule list
    RegEx: EnrichedRegex list
}

let get_enriched_rule (empty_generators: int list) (firsts: int list list) (follows: int list list) (rule: Rule) =
    if (empty_generators |> List.contains rule.Token.Index) 
        then 
            let directive_symbols =
                rule.Derivation 
                |> List.ofSeq
                |> get_sequence_firsts empty_generators firsts
                |> List.append follows[rule.Token.Index]
                |> List.distinct
            { Token = rule.Token; Derivation = rule.Derivation |> List.ofSeq; DirectiveSymbols = directive_symbols }
        else 
            let directive_symbols =
                rule.Derivation
                |> List.ofSeq
                |> get_sequence_firsts empty_generators firsts
            { Token = rule.Token; Derivation = rule.Derivation |> List.ofSeq; DirectiveSymbols = directive_symbols }

let rec get_sub_enriched_regex (pattern: char list) (depth: int) =
    match depth, pattern with
    | _, [] -> []
    | 0, '|'::tail -> '$'::'|'::'^'::(get_sub_enriched_regex tail depth)
    | _, '('::tail -> '('::(get_sub_enriched_regex tail (depth + 1))
    | _, ')'::tail -> ')'::(get_sub_enriched_regex tail (depth - 1))
    | _, c::tail -> c::(get_sub_enriched_regex tail depth) 

let get_enriched_regex (regex: RegularExpression) =
    let new_pattern =
        get_sub_enriched_regex (regex.Pattern |> List.ofSeq) 0
        |> List.toArray
        |> System.String
    { Token = regex.Token; Pattern = "^" + new_pattern + "$" }

let rec private compute_rules (empty_generators: int list) (firsts: int list list) (follows: int list list) (rules: Rule list) =
    match rules with
    | [] -> []
    | rule::tail -> (rule |> get_enriched_rule empty_generators firsts follows)::(tail |> compute_rules empty_generators firsts follows)

let rec private compute_regex (regexs: RegularExpression list) =
    match regexs with
    | [] -> []
    | rg::tail -> (rg |> get_enriched_regex)::(tail |> compute_regex)

[<CompiledName("Get")>]
let public get_lookahead_set (g: Grammar) =
    let empty_generators = g |> EmptyAnalyser.get_empty_generators
    let firsts = (g, empty_generators) ||> FirstsAnalyser.get_firsts
    let follows = (g, empty_generators, firsts) |||> FollowsAnalyser.get_follows
    {
        Axiom = g.Axiom
        EOF = { Name = "EOF"; Index = g.Terminals.Count - 1 }
        Terminals = g.Terminals |> List.ofSeq
        NonTerminals = g.NonTerminals |> List.ofSeq
        Rules = g.Rules |> List.ofSeq |> compute_rules empty_generators firsts follows;
        RegEx = g.RegularExpressions |> List.ofSeq |> compute_regex
    }