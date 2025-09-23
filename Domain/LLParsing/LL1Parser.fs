module Nt.SyntaxAnalyser.LLParsing.LL1Parser

open Nt.SyntaxParser.Parsing.Structures
open Nt.SyntaxParser.Syntax.Structures
open Nt.SyntaxAnalyser.Utils

/// Eliminates direct left recursivity for a given non-terminal in the grammar
let public eliminate_direct_recursivity (tokens: TokensList) (s:NonTerminal) (g:Grammar) =
    "_rec" |> extend_token tokens s.Index |> add_to_tokens tokens |> add_to_tokens g.NonTerminals |> ignore
    let newindex = g.NonTerminals.Count - 1

    let rules = List.ofSeq(g.Rules)
        
        
    (*Constructs new rules with same symbol and derivation but newsymbol at the end*)
    rules |> List.filter (fun (r: Rule) -> (r.Token.Index = s.Index) && (r.Derivation[0].Index <> s.Index) )
    |> List.map (fun (r: Rule) -> 
        g.Rules.Remove(r) |> ignore
        create_rule g.Terminals g.NonTerminals r.Token.Index r.Derivation
        |> add_non_terminal_to_derivation newindex
        |> add_rule_to_grammar g)
    |> ignore

    (*Constructs new rules with newsymbol and derivation removed from the first item (recursive one)*)
    rules |> List.filter (fun (r: Rule) -> (r.Token.Index = s.Index) && (r.Derivation[0].Index = s.Index))
    |> List.map( fun (r: Rule) -> 
        g.Rules.Remove(r) |> ignore
        create_rule g.Terminals g.NonTerminals newindex r.Derivation 
        |> add_non_terminal_to_derivation newindex
        |> add_rule_to_grammar g)
    |> ignore

    (*Adds an empty rule with newsymbol*)
    newindex |> create_empty_rule g.Terminals g.NonTerminals
    |> add_rule_to_grammar g

/// Eliminates indirect left recursivity for a given non-terminal in the grammar
let public eliminate_recursivity (axiom: int) (firsttoken: int) (g: Grammar) =
    let rules = g.Rules |> List.ofSeq
    let recursive_rules = rules |> List.filter(fun (r: Rule) -> (r.Token.Index = axiom) && (r.Derivation[0].Index = firsttoken))
    let symbol_rules = rules |> List.filter(fun (r: Rule) -> (r.Token.Index = firsttoken))

    recursive_rules |> List.map(fun (rr: Rule) ->
        g.Rules.Remove(rr) |> ignore
        symbol_rules |> List.map(fun (sr: Rule) ->
        create_rule g.Terminals g.NonTerminals axiom sr.Derivation
        |> expand_rule_derivation (rr.Derivation |> List.ofSeq |> List.tail)
        |> add_rule_to_grammar g)
    ) |> ignore