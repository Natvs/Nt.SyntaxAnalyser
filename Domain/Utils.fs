module Nt.SyntaxAnalyser.Utils

open Nt.SyntaxParser.Parsing.Structures
open Nt.SyntaxParser.Syntax.Structures

/// Creates an empty rule for a given symbol index
let public create_empty_rule (terminals: TokensList) (nonTerminals: TokensList) (symbolIndex:int) : Rule =
    let rule = new Rule(terminals, nonTerminals)
    rule.SetToken(symbolIndex, -1)
    rule

/// Creates a rule for a given symbol index and derivation
let public create_rule (terminals: TokensList) (nonTerminals: TokensList) (symbolIndex:int) (derivation:Derivation) : Rule =
    let rule = new Rule(terminals, nonTerminals)
    rule.SetToken(symbolIndex, -1)
    List.ofSeq (derivation)
    |> List.map (fun (t:GrammarToken) -> 
        match t with
            | :? Terminal -> rule.AddTerminal(t.Index, -1) 
            | :? NonTerminal -> rule.AddNonTerminal(t.Index, -1)
            | _ -> ()
    ) |> ignore
    rule

/// Adds a non-terminal to the derivation of a rule
let public add_non_terminal_to_derivation (index:int) (rule: Rule) : Rule =
    rule.AddNonTerminal(index, -1)
    rule

let public expand_rule_derivation (tokens: GrammarToken list) (rule: Rule) : Rule =
    tokens |> List.map (fun (t:GrammarToken) -> rule.Derivation.Add(t)) |> ignore
    rule

/// Adds a rule to the grammar
let public add_rule_to_grammar (g: Grammar) (rule: Rule) : Rule =
    g.Rules.Add(rule)
    rule

/// Extends the name of a token at a given index
let public extend_token (tokens: TokensList) (index: int) (stringExtension: string) : Token =
    new Token(tokens.Item(index).Name + stringExtension)

/// Adds a token to a tokens list
let public add_to_tokens (tokens: TokensList) (token: Token) : Token =
    tokens.Add(token)
    token
