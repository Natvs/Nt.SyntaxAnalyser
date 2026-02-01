module Nt.Syntax.LLAnalysing.EmptyAnalyser

open Nt.Syntax.Structures
open Nt.Parser.Symbols
open Nt.Syntax.LLAnalysing.Utils

// Check if a symbol has a direct empty rule
let private is_direct_empty_rule_symbol (rules: Rule list) (symbol: ISymbol) =
    rules
    |> List.exists( fun r -> r.Token.Symbol = symbol && r.Derivation.Count = 0 )

// Get all non-terminals that have direct empty rules
let private get_direct_empty_generators (rules: Rule list) (non_terminals: ISymbol list) =
    non_terminals
    |> List.filter(fun s ->  is_direct_empty_rule_symbol rules s)

// Check if a symbol has any rule that can generate the empty string
let private is_emtpy_rule_symbol (rules: Rule list) (empty_generators: ISymbol list) (symbol: ISymbol) =
    rules
    |> List.exists (fun r -> 
        r.Token.Symbol = symbol && 
        r.Derivation.GetTokens() |> List.ofSeq |> is_sequence_empty_generator empty_generators)

// Perform one iteration to compute new empty generators
let rec private get_empty_generators (rules: Rule list) (empty_generators: ISymbol list) (non_terminals: ISymbol list) =
    match non_terminals with
    | [] -> []
    | index::tail when index |> is_emtpy_rule_symbol rules empty_generators -> index::(tail |> get_empty_generators rules empty_generators)
    | _::tail -> tail |> get_empty_generators rules empty_generators

// Recursively compute all empty generators until no new ones are found
let rec private get_all_empty_generators (rules: Rule list) (empty_generators: ISymbol list) (non_terminals: ISymbol list) =
    let new_empty_generators = non_terminals |> get_empty_generators rules empty_generators
    if new_empty_generators.Length <> empty_generators.Length 
        then non_terminals |> get_all_empty_generators rules new_empty_generators
        else new_empty_generators

// Public function to get all empty generators from a grammar
[<CompiledName("Analyse")>]
let public analyse (g: Grammar) =
    let rules = g.Rules |> List.ofSeq
    let non_terminals = g.NonTerminals.GetSymbols() |> List.ofSeq
    let empty_generators = non_terminals |> get_direct_empty_generators rules
    non_terminals |> get_all_empty_generators rules empty_generators



