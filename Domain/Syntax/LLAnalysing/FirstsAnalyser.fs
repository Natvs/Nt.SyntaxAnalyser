module Nt.Syntax.LLAnalysing.FirstsAnalyser

open Nt.Parser.Symbols
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.Utils

/// Perform one iteration to compute new firsts for all rules
let rec private get_firsts (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (rules: Rule list) =
    match rules with
    | [] -> firsts
    | rule::tail -> 
        let local_new_firsts = (rule.Derivation |> List.ofSeq) |> get_sequence_firsts empty_generators firsts
        if local_new_firsts.IsEmpty then
            tail |> get_firsts empty_generators firsts
        else
            let _get_new_firsts_set (opt: Set<SyntaxSymbol> option) = 
                match opt with
                | Some lst -> Some( lst + local_new_firsts )
                | None -> Some( local_new_firsts )
            let new_firsts = firsts.Change (rule.Token.Symbol :?> SyntaxSymbol, _get_new_firsts_set)
            tail |> get_firsts empty_generators new_firsts

/// Recursively compute all firsts until no new ones are found
let rec private get_all_firsts (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (rules: Rule list) =
    let new_firsts = rules |> get_firsts empty_generators firsts
    if (firsts, new_firsts) ||> compare_maps 
        then new_firsts
        else rules |> get_all_firsts empty_generators new_firsts


/// Public function to compute firsts symbols for a grammar
[<CompiledName("Analyse")>]
let public analyse (g: Grammar) (empty_generators: ISymbol list) =
    g.Rules
    |> List.ofSeq
    |> get_all_firsts empty_generators Map.empty


