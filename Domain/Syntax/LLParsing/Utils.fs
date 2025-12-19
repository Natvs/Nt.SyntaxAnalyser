module Nt.Syntax.LLParsing.Utils

open Nt.Parsing.Structures
open Nt.Syntax.Structures

(*- - - - - OPERATIONS ON TOKENS - - - - -*)

/// Compares two tokens
let internal compare_tokens (token1: GrammarToken) (token2: GrammarToken) =
    token1.Index = token2.Index && token1.Type = token2.Type

/// Compare the token with a given type and index
let internal compare_token (index: int) (t: GrammarTokenType) (token: GrammarToken) =
    token.Index = index && token.Type = t

/// Creates a symbol from its string representation
let internal create_symbol (name: string) =
    new Symbol(name)

/// Extends the name of a token at a given index
let internal extend_symbol (symbols: SymbolsList) (index: int) (stringExtension: string) =
    let rec get_unique_name (root: string) (id: int) (strings: string list) =
        let newtoken = root + id.ToString()
        match strings |> List.contains newtoken with
            | false -> newtoken
            | true -> strings |> get_unique_name root (id + 1)

    let token_name =
        symbols
        |> List.ofSeq
        |> List.map(fun t -> t.Name)
        |> get_unique_name (symbols.Item(index).Name + stringExtension) 1

    new Symbol(token_name)

/// Defines if a symbol is unique in a list of symbols
let internal is_symbol_existing (symbols: SymbolsList) (symbol: string)  =
    symbols
    |> List.ofSeq
    |> List.map (fun t -> t.Name)
    |> List.contains symbol


/// Adds a token to a tokens list
let internal add_to_symbols (symbols: SymbolsList) (symbol: Symbol)  =
    symbols.Add(symbol)
    symbol




(*- - - - - OPERATIONS ON RULES - - - - -*)

/// Creates an empty rule for a given symbol index
let internal create_empty_rule (terminals: SymbolsList) (nonTerminals: SymbolsList) (symbolIndex:int) =
    let rule = new Rule(terminals, nonTerminals)
    rule.SetToken(symbolIndex, -1)
    rule

/// Creates a rule for a given symbol index and derivation
let internal create_rule (terminals: SymbolsList) (nonTerminals: SymbolsList) (symbolIndex:int) (derivation:GrammarToken list) =
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

/// Removes a collection of rules from the grammar
let internal remove_rules_from_grammar (g: Grammar) (rules: Rule list) =
    rules
    |> List.iter (fun r -> g.Rules.Remove r |> ignore)
    g

/// Adds a non-terminal to the derivation of a rule
let internal add_non_terminal_to_derivation (index:int) (rule: Rule) : Rule =
    rule.AddNonTerminal(index, -1)
    rule

/// Adds a symbol to a rule's derivation
let internal expand_rule_derivation (tokens: GrammarToken list) (rule: Rule) : Rule =
    tokens |> List.map (fun (t:GrammarToken) -> rule.Derivation.Add(t)) |> ignore
    rule

/// Removes the first symbol of a derivation pattern
let internal remove_first_token (p: GrammarToken list) =
    match p with
        | a::tail -> tail
        | _ -> []





(*- - - - - OPERATIONS ON REGULAR EXPRESSIONS - - - - -*)

/// Adds a rule to the grammar
let internal add_rule_to_grammar (g: Grammar) (rule: Rule) = 
    g.Rules.Add(rule) |> ignore
    g

/// Creates a regular expressing for a given symbol and pattern
let internal create_regex (nonTerminals: SymbolsList) (symbolIndex:int) (pattern: string) =
    let regex = new RegularExpression(nonTerminals)
    regex.SetToken(symbolIndex, -1)
    regex.AddSymbols(pattern)
    regex

/// Adds a regular expression to the grammar
let internal add_regex_to_grammar (g: Grammar) (regex: RegularExpression) =
    g.RegularExpressions.Add(regex) |> ignore
    g

/// Removes a collection of regular expressions from the grammar
let internal remove_regexs_from_grammar (g: Grammar) (regexs: RegularExpression list) =
    regexs
    |> List.iter (fun rg -> g.RegularExpressions.Remove rg |> ignore)
    g