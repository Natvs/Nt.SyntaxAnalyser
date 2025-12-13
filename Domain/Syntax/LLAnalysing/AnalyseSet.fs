module Nt.Syntax.LLAnalysing.LL1AnalyseSet

open Nt.Parsing.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing
open Nt.Syntax.LLAnalysing.Utils

type EnrichedRule = {
    Rule : Rule
    Symbols : int list
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
    RegEx: RegularExpression list
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
            { Rule = rule; Symbols = directive_symbols }
        else 
            let directive_symbols =
                rule.Derivation
                |> List.ofSeq
                |> get_sequence_firsts empty_generators firsts
            { Rule = rule; Symbols = directive_symbols }

let rec private compute_rules (empty_generators: int list) (firsts: int list list) (follows: int list list) (rules: Rule list) =
    match rules with
    | [] -> []
    | rule::tail -> (rule |> get_enriched_rule empty_generators firsts follows)::(tail |> compute_rules empty_generators firsts follows)

let rec private compute_regex (regexs: RegularExpression list) =
    match regexs with
    | [] -> []
    | rg::tail -> rg::(tail |> compute_regex)

[<CompiledName("Get")>]
let public get_lookahead_set (g: Grammar) =
    let empty_generators = g |> EmptyAnalyser.get_empty_generators
    let firsts = (g, empty_generators) ||> FirstsAnalyser.get_firsts
    let follows = (g, empty_generators, firsts) |||> FollowsAnalyser.get_follows
    let rules = g.Rules |> List.ofSeq
    let regexs = g.RegularExpressions |> List.ofSeq
    {
        Axiom = g.Axiom
        EOF = { Name = "EOF"; Index = g.Terminals.Count - 1 }
        Terminals = g.Terminals |> List.ofSeq
        NonTerminals = g.NonTerminals |> List.ofSeq
        Rules = rules |> compute_rules empty_generators firsts follows;
        RegEx = regexs |> compute_regex
    }