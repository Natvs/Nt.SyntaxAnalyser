module Nt.Syntax.LLAnalysing.EmptyGenerators

open Nt.Syntax.Structures

let private is_direct_empty_rule_symbol (rules: Rule list) (index: int) =
    rules
    |> List.exists( fun r -> r.Token.Index = index && r.Derivation.Count = 0 )

let private direct_empty_generators (rules: Rule list) (non_terminals: int list): int list =
    non_terminals
    |> List.filter(fun s ->  is_direct_empty_rule_symbol rules s)

let private is_rule_empty_generator (empty_generators: int list) (rule: Rule) =
    rule.Derivation 
    |> List.ofSeq
    |> List.forall ( fun s -> s.Type = GrammarTokenType.NonTerminal && empty_generators |> List.contains s.Index )

let private is_emtpy_rule_symbol (rules: Rule list) (empty_generators: int list) (index: int) =
    rules
    |> List.exists (fun r -> r.Token.Index = index && r |> is_rule_empty_generator empty_generators)

let rec private compute_empty_generators (rules: Rule list) (empty_generators: int list) (non_terminals: int list): int list =
    match non_terminals with
    | [] -> []
    | index::tail when index |> is_emtpy_rule_symbol rules empty_generators -> index::(tail |> compute_empty_generators rules empty_generators)
    | _::tail -> tail |> compute_empty_generators rules empty_generators

let rec private compute_all_empty_generators (rules: Rule list) (empty_generators: int list) (non_terminals: int list) =
    let new_empty_generators = non_terminals |> compute_empty_generators rules empty_generators
    if new_empty_generators.Length <> empty_generators.Length 
        then non_terminals |> compute_all_empty_generators rules new_empty_generators
        else new_empty_generators


[<CompiledName("Analyse")>]
let public get_empty_generators (g: Grammar) =
    let rules = g.Rules |> List.ofSeq
    let non_terminals = [0 .. g.NonTerminals.Count-1]
    let empty_generators = non_terminals |> direct_empty_generators rules
    non_terminals |> compute_all_empty_generators rules empty_generators



