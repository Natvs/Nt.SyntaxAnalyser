module Nt.Syntax.LLParsing.Derecursivation

open Nt.Parser.Symbols
open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils

let private need_direct_derecursivation (rules: Rule list) (s: ISymbol): bool =
    rules
    |> List.ofSeq
    |> List.filter(fun r -> r.Token.Symbol = s && r.Derivation.Count > 0)
    |> List.filter(fun r -> r.Derivation[0] |> compare_token s GrammarTokenType.NonTerminal)
    |> List.isEmpty = false

/// Eliminates direct left recursivity for a given non-terminal in the grammar
let private eliminate_direct_recursivity (s: ISymbol) (g:Grammar) =
    let new_symbol =
        s
        |> get_extended_name "_rec" g.NonTerminals
        |> add_as_non_terminal g

    let rules = 
        g.Rules
        |> List.ofSeq
        |> List.filter (fun r -> r.Token.Symbol = s && r.Derivation.Count > 0)
         
    (*Add an empty rule with the new symbol*)
    g
    |> add_empty_rule_to_grammar new_symbol
    |> ignore
         
    (*Add new symbol to the end of non-recursive rules*)
    rules
    |> List.filter(fun r -> r.Derivation[0] |> compare_token s GrammarTokenType.NonTerminal = false )
    |> List.iter (fun r -> 
        r
        |> add_non_terminal_to_derivation new_symbol
        |> ignore)

    (*Remove recursive rules and add new rules with no direct recursion*)
    rules 
    |> List.filter (fun r -> r.Derivation[0] |> compare_token s GrammarTokenType.NonTerminal)
    |> List.iter( fun r -> 
        let new_derivation =
            r
            |> add_non_terminal_to_derivation new_symbol
            |> remove_first_token

        g 
        |> remove_rule_from_grammar r
        |> add_rule_to_grammar new_symbol new_derivation
        |> ignore)

    g

/// Eliminates indirect left recursivity for a given non-terminal in the grammar
let private eliminate_symbol_indirect_recursivity (axiom: ISymbol) (first_symbol: ISymbol) (g: Grammar) =
    let rules = g.Rules |> List.ofSeq

    let recursive_rules = 
        rules 
        |> List.filter(fun r -> r.Token.Symbol = axiom && r.Derivation.Count > 0)
        |> List.filter(fun r -> r.Derivation[0] |> compare_token first_symbol GrammarTokenType.NonTerminal)

    let symbol_rules = 
        rules 
        |> List.filter(fun r -> r.Token.Symbol = first_symbol)

    (*For all of the recursive rules, remove the rule and add an equivalent non recursive one*)
    recursive_rules 
    |> List.iter(fun (r: Rule) ->

        g
        |> remove_rule_from_grammar r
        |> ignore

        let extra_derivation = r |> remove_first_token

        symbol_rules
        |> List.iter(fun sr ->
            g
            |> add_rule_to_grammar axiom (sr.Derivation |> List.ofSeq)
            |> expand_rule_derivation extra_derivation
            |> ignore)
    )

    g

let rec private eliminate_indirect_recursivity (left: ISymbol list) (seen: ISymbol list) (g: Grammar) =
    match left with
    | [] -> g
    | axiom::tail -> 
        seen
        |> List.iter(fun nt -> g |> eliminate_symbol_indirect_recursivity axiom nt |> ignore)
        |> ignore
        eliminate_indirect_recursivity tail (axiom::seen) g

[<CompiledName("EliminateRecursivity")>]
let public eliminate_recursivity (g: Grammar) =
    let nonTerminals = 
        g.NonTerminals.GetSymbols() 
        |> List.ofSeq
        |> List.map(fun s -> s :?> Nt.Syntax.LLAnalysing.Utils.SyntaxSymbol)
        |> Nt.Syntax.LLAnalysing.Utils.get_ordered_symbols
        |> List.map(fun s -> s :> ISymbol)

    (*Get symbols that generate regular expressions*)
    let regex_symbols =
        g.RegularExpressions
        |> List.ofSeq
        |> List.map (_.Token.Symbol)
        |> List.distinct

    g
    |> eliminate_indirect_recursivity nonTerminals []
    |> ignore  

    nonTerminals 
    |> List.iter(fun s -> 
        match s |> need_direct_derecursivation (g.Rules |> List.ofSeq) with
            | false -> ()
            | true -> eliminate_direct_recursivity s g |> ignore
        )

    g