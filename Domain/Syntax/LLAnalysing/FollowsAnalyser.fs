module Nt.Syntax.LLAnalysing.FollowsAnalyser

open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.Utils

let rec private is_empty_generator (empty_generators: int list) (sequence: GrammarToken list) =
    match sequence with
    | [] -> true
    | symbol::_ when symbol.Type = GrammarTokenType.Terminal -> false
    | symbol::tail when empty_generators |> List.contains symbol.Index -> tail |> is_empty_generator empty_generators
    | _ -> false

let rec private get_sequence_firsts (empty_generators: int list) (firsts: int list list) (sequence: GrammarToken list) =
    match sequence with
    | [] -> []
    | token::_ when token.Type = GrammarTokenType.Terminal -> token.Index::[]
    | token::tail when empty_generators |> List.contains token.Index -> firsts[token.Index]@(get_sequence_firsts empty_generators firsts tail) |> List.distinct
    | token::_ -> firsts[token.Index]

let rec private compute_sequence (empty_generators: int list) (firsts: int list list) (follows: int list list) (rule_symbol: int) (derivation: GrammarToken list) =
    match derivation with
    | [] -> follows
    | token::tail when token.Type = GrammarTokenType.Terminal -> (rule_symbol, tail) ||> compute_sequence empty_generators firsts follows
    | token::tail when tail |> is_empty_generator empty_generators -> 
        let new_follows = 
            follows 
            |> List.mapi (fun i value -> if i = token.Index then follows[rule_symbol]@(tail |> get_sequence_firsts empty_generators firsts)@value |> List.distinct else value)
        (rule_symbol, tail) ||> compute_sequence empty_generators firsts new_follows
    | token::tail -> 
        let new_follows = 
            follows
            |> List.mapi (fun i value -> if i = token.Index then (tail |> get_sequence_firsts empty_generators firsts)@value |> List.distinct else value)
        (rule_symbol, tail) ||> compute_sequence empty_generators firsts new_follows

let rec private compute_follows (empty_generators: int list) (firsts: int list list) (follows: int list list) (rules: Rule list) =
    match rules with
    | [] -> follows
    | rule::tail ->
        let new_follows = (rule.Token.Index, rule.Derivation |> List.ofSeq) ||> compute_sequence empty_generators firsts follows
        tail |> compute_follows empty_generators firsts new_follows

let rec private compute_all_follows (empty_generators: int list) (firsts: int list list) (follows: int list list) (rules: Rule list) =
    let new_follows = rules |> compute_follows empty_generators firsts follows
    if (follows, new_follows) ||> compare_lists
        then new_follows
        else rules |> compute_all_follows empty_generators firsts new_follows

let rec private init_follows (g: Grammar) =
    let new_index = g.Terminals.Add("EOF")
    [for _ in [1 .. g.NonTerminals.Count] -> 0]
    |> List.mapi (fun i _ -> if g.Axiom = i then new_index::[] else [])


[<CompiledName("Analyse")>]
let public get_follows (g: Grammar) (empty_generators: int list) (firsts: int list list) =
    g.Rules
    |> List.ofSeq
    |> compute_all_follows empty_generators firsts (g |> init_follows)