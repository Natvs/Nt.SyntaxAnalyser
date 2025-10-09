module Nt.SyntaxAnalyser.LLParsing.Derecursivation

open Nt.SyntaxParser.Parsing.Structures
open Nt.SyntaxParser.Syntax.Structures
open Nt.SyntaxAnalyser.Utils

let private need_direct_derecursivation (rules: Rule list) (s:int): bool =
    rules
    |> List.ofSeq
    |> List.filter(fun r -> r.Token.Index = s && r.Derivation[0] |> compare_token s GrammarTokenType.NonTerminal)
    |> List.isEmpty = false

/// Eliminates direct left recursivity for a given non-terminal in the grammar
[<CompiledName("EliminateDirectRecursivity")>]
let public eliminate_direct_recursivity (s: int) (g:Grammar) =
    "_rec" 
    |> extend_token g.NonTerminals s 
    |> add_to_tokens g.NonTerminals 
    |> ignore
    let newindex = g.NonTerminals.Count - 1
    let rules = List.ofSeq(g.Rules)
             
    (*Constructs new rules with same symbol and derivation but newsymbol at the end*)
    rules 
    |> List.filter (fun r -> (r.Token.Index = s) && (r.Derivation[0] |> compare_token s GrammarTokenType.NonTerminal) = false )
    |> List.iter (fun r -> add_non_terminal_to_derivation newindex r |> ignore)

    (*Constructs new rules with newsymbol and derivation removed from the first item (recursive one)*)
    rules 
    |> List.filter (fun (r: Rule) -> (r.Token.Index = s) && r.Derivation[0].Type = GrammarTokenType.NonTerminal && r.Derivation[0].Index = s)
    |> List.iter( fun (r: Rule) -> 
        g.Rules.Remove(r) |> ignore
        r.Derivation
        |> List.ofSeq
        |> remove_first_symbol
        |> create_rule g.Terminals g.NonTerminals newindex
        |> add_non_terminal_to_derivation newindex
        |> add_rule_to_grammar g)

    (*Adds an empty rule with newsymbol*)
    newindex |> create_empty_rule g.Terminals g.NonTerminals
    |> add_rule_to_grammar g

    g

/// Eliminates indirect left recursivity for a given non-terminal in the grammar
[<CompiledName("EliminateIndirectRecursivity")>]
let public eliminate_indirect_recursivity (axiom: int) (firsttoken: int) (g: Grammar) =
    let rules = g.Rules |> List.ofSeq
    let recursive_rules = rules |> List.filter(fun (r: Rule) -> (r.Token.Index = axiom) && (r.Derivation[0] |> compare_token firsttoken GrammarTokenType.NonTerminal))
    let symbol_rules = rules |> List.filter(fun (r: Rule) -> (r.Token.Index = firsttoken))

    recursive_rules 
    |> List.iter(fun (rr: Rule) ->
        g.Rules.Remove(rr) |> ignore
        symbol_rules |> List.iter(fun (sr: Rule) ->
            create_rule g.Terminals g.NonTerminals axiom (List.ofSeq sr.Derivation)
            |> expand_rule_derivation (rr.Derivation |> List.ofSeq |> List.tail)
            |> add_rule_to_grammar g)
    )

    g

[<CompiledName("EliminateRecursivity")>]
let public eliminate_recursivity (g: Grammar) =
    let nonTerminals = g.NonTerminals |> List.ofSeq

    nonTerminals |> List.map(fun (t: Token) ->
        let current_index = g.NonTerminals.IndexOf t
        nonTerminals
        |> List.filter(fun (nt: Token) -> (g.NonTerminals.IndexOf nt.Name) < current_index)
        |> List.map(fun (pt: Token) ->
            let compare_index = g.NonTerminals.IndexOf pt
            eliminate_indirect_recursivity current_index compare_index g)
        |> ignore)
    |> ignore  

    nonTerminals |> List.iter(fun (t: Token) -> 
        let current_index = g.NonTerminals.IndexOf t
        match need_direct_derecursivation (g.Rules |> List.ofSeq) current_index with
            | false -> ()
            | true -> eliminate_direct_recursivity current_index g |> ignore
        )

    g
