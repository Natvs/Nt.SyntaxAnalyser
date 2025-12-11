module Nt.Syntax.LLAnalysing.FirstsAnalyser

open Nt.Syntax.Structures

let rec private compare_firsts (old_firsts: int list list) (new_firsts: int list list) =
    match old_firsts, new_firsts with
    | [], [] -> true
    | old_head::old_tail, new_head::new_tail when old_head.Length = new_head.Length -> compare_firsts old_tail new_tail
    | _ -> false

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
        let local_new_firsts =
            (rule.Derivation |> List.ofSeq)
            |> compute_sequence empty_generators firsts
            |> List.filter (fun s -> (firsts[rule.Token.Index] |> List.contains s) = false)
        let new_firsts = firsts |> List.mapi (fun i value -> if rule.Token.Index = i then local_new_firsts@firsts[i] else firsts[i] )
        tail |> compute_firsts empty_generators new_firsts

let rec private compute_all_firsts (empty_generators: int list) (firsts: int list list) (rules: Rule list) =
    let new_firsts = rules |> compute_firsts empty_generators firsts
    if (firsts, new_firsts) ||> compare_firsts
        then new_firsts
        else rules |> compute_all_firsts empty_generators new_firsts

[<CompiledName("Analyse")>]
let public get_firsts (g: Grammar) (empty_generators: int list) =
    let rules = g.Rules |> List.ofSeq
    rules |> compute_all_firsts empty_generators [for _ in [1 .. g.NonTerminals.Count] -> []]


