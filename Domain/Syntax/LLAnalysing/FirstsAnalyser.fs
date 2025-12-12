module Nt.Syntax.LLAnalysing.FirstsAnalyser

open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.Utils

let rec private compute_sequence (empty_generators: int list) (firsts: int list list) (sequence: GrammarToken list) =
    match sequence with
    | [] -> []
    | symbol::_ when symbol.Type = GrammarTokenType.Terminal -> symbol.Index::[]
    | symbol::tail when empty_generators |> List.contains symbol.Index -> firsts[symbol.Index]@(tail |> compute_sequence empty_generators firsts)
    | symbol::_ -> firsts[symbol.Index]

let rec private compute_firsts (empty_generators: int list) (firsts: int list list) (rules: Rule list) =
    match rules with
    | [] -> firsts
    | rule::tail -> 
        let local_new_firsts = (rule.Derivation |> List.ofSeq) |> compute_sequence empty_generators firsts
        let new_firsts = firsts |> List.mapi (fun i value -> if rule.Token.Index = i then local_new_firsts@value |> List.distinct else value )
        tail |> compute_firsts empty_generators new_firsts

let rec private compute_all_firsts (empty_generators: int list) (firsts: int list list) (rules: Rule list) =
    let new_firsts = rules |> compute_firsts empty_generators firsts
    if (firsts, new_firsts) ||> compare_lists
        then new_firsts
        else rules |> compute_all_firsts empty_generators new_firsts

[<CompiledName("Analyse")>]
let public get_firsts (g: Grammar) (empty_generators: int list) =
    g.Rules
    |> List.ofSeq
    |> compute_all_firsts empty_generators [for _ in [1 .. g.NonTerminals.Count] -> []]


