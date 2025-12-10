module Nt.Syntax.Utils

open Nt.Parsing.Structures
open Nt.Syntax.Structures

/// Compares two tokens
let internal compare_tokens (token1: GrammarToken) (token2: GrammarToken) =
    token1.Index = token2.Index && token1.Type = token2.Type

let internal compare_token (index: int) (t: GrammarTokenType) (token: GrammarToken) =
    token.Index = index && token.Type = t

/// Creates an empty rule for a given symbol index
let internal create_empty_rule (terminals: SymbolsList) (nonTerminals: SymbolsList) (symbolIndex:int) : Rule =
    let rule = new Rule(terminals, nonTerminals)
    rule.SetToken(symbolIndex, -1)
    rule

/// Creates a rule for a given symbol index and derivation
let internal create_rule (terminals: SymbolsList) (nonTerminals: SymbolsList) (symbolIndex:int) (derivation:GrammarToken list) : Rule =
    let rule = new Rule(terminals, nonTerminals)
    rule.SetToken(symbolIndex, -1)
    derivation
    |> List.map (fun (t:GrammarToken) -> 
        match t with
            | :? Terminal -> rule.AddTerminal(t.Index, -1) 
            | :? NonTerminal -> rule.AddNonTerminal(t.Index, -1)
            | _ -> ()
    ) |> ignore
    rule

/// Adds a non-terminal to the derivation of a rule
let internal add_non_terminal_to_derivation (index:int) (rule: Rule) : Rule =
    rule.AddNonTerminal(index, -1)
    rule

/// Adds a symbol to a rule's derivation
let internal expand_rule_derivation (tokens: GrammarToken list) (rule: Rule) : Rule =
    tokens |> List.map (fun (t:GrammarToken) -> rule.Derivation.Add(t)) |> ignore
    rule

/// Adds a rule to the grammar
let internal add_rule_to_grammar (g: Grammar) (rule: Rule) = 
    g.Rules.Add(rule) |> ignore
    g

/// Extends the name of a token at a given index
let internal extend_token (tokens: SymbolsList) (index: int) (stringExtension: string) : Symbol =
    let rec get_unique_name (root: string) (id: int) (strings: string list) =
        let newtoken = root + id.ToString()
        match strings |> List.contains newtoken with
            | false -> newtoken
            | true -> strings |> get_unique_name root (id + 1)

    let token_name =
        tokens
        |> List.ofSeq
        |> List.map(fun t -> t.Name)
        |> get_unique_name (tokens.Item(index).Name + stringExtension) 1

    new Symbol(token_name)


/// Adds a token to a tokens list
let internal add_to_tokens (tokens: SymbolsList) (token: Symbol) : Symbol =
    tokens.Add(token)
    token

/// Removes the first symbol of a derivation pattern
let internal remove_first_symbol (p: GrammarToken list): GrammarToken list =
    match p with
        | a::tail -> tail
        | _ -> []