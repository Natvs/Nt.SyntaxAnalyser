module Nt.Syntax.LLParsing.Utils

open Nt.Parser.Symbols
open Nt.Syntax.Structures
open Nt.Syntax.Builders

(*- - - - - OPERATIONS ON TOKENS - - - - -*)

/// Compares two tokens
let internal compare_tokens (token1: GrammarToken) (token2: GrammarToken) =
    token1.Symbol = token2.Symbol && token1.Type = token2.Type

/// Compare the token with a given type and index
let internal compare_token (symbol: ISymbol) (t: GrammarTokenType) (token: GrammarToken) =
    token.Symbol = symbol && token.Type = t

/// Get a new name by extending an existing one with a given string extension. The method ensures unicity of the new name.
let internal get_extended_name (stringExtension: string) (symbols: ISymbol list) (symbol: ISymbol) =
    let rec get_unique_name (root: string) (id: int) (names: string list) =
        let new_name = root + id.ToString()
        match names |> List.contains new_name with
            | false -> new_name
            | true -> names |> get_unique_name root (id + 1)

    let token_name =
        symbols
        |> List.map(fun t -> t.Name)
        |> get_unique_name (symbol.Name + stringExtension) 1

    token_name

/// Define if a symbol is unique in a list of symbols
let internal is_symbol_existing (symbols: ISymbol list) (symbol: string)  =
    symbols
    |> List.map (fun t -> t.Name)
    |> List.contains symbol

/// Add a symbol as a terminal of a grammar
let internal add_as_terminal (g: Grammar) (name: string) =
    g.GetBuilder().AddTerminal(name) |> ignore
    g.Terminals.Get(name)

/// Add a symbol as a non-terminal of a grammar
let internal add_as_non_terminal (g: Grammar) (name: string) =
    g.GetBuilder().AddNonTerminal(name) |> ignore
    g.NonTerminals.Get(name)

/// Remove a terminal from a grammar
let internal remove_terminal_from_grammar (name: string) (g: Grammar) =
    g.GetBuilder().RemoveTerminal(name).Build()

/// Remove a list of symbol from a grammar
let internal remove_symbols_from_grammar (remover: string -> Grammar -> Grammar) (names: string list) (g: Grammar) =
    names
    |> List.iter (fun name -> g |> remover name |> ignore)
    g

/// Remove a non-terminal from a grammmar
let internal remove_non_terminal_from_grammar (name: string) (g: Grammar) =
    g.GetBuilder().RemoveNonTerminal(name).Build()

// Get a list of symbols used in rules derivations, with a selective method to filter symbols
let get_used_symbols (selective: GrammarToken -> bool) (g: Grammar) =

    let rec get_used_pattern_symbols (tokens: GrammarToken list) =
        match tokens with
        | [] -> []
        | token::tail when selective token -> (token.Symbol)::(tail |> get_used_pattern_symbols)
        | _::tail -> tail |> get_used_pattern_symbols

    let rec get_used_rules_symbols (rules: Rule list) =
         match rules with
            | [] -> []
            | rule::tail ->
                let used =
                    rule.Derivation
                    |> List.ofSeq
                    |> get_used_pattern_symbols
                used@(tail |> get_used_rules_symbols)

    g.Axiom.Symbol::(g.Rules |> List.ofSeq |> get_used_rules_symbols) |> List.distinct    

/// Get the complementary set, meaning a list containing only the symbols that do not appear in the symbols list
let get_complementary (universe: ISymbol list) (symbols: ISymbol list) =
    universe |> List.except symbols

(*- - - - - OPERATIONS ON RULES - - - - -*)

/// Add an empty rule to the grammar
let internal add_empty_rule_to_grammar (symbol: ISymbol) (g: Grammar) = 
    let rule = Rule(g)
                .GetBuilder()
                .SetToken(NonTerminal(symbol, -1)) 
                .Build()
    g.GetBuilder().Add(rule) |> ignore
    rule

/// Add a rule with a derivation to the grammar
let internal add_rule_to_grammar (symbol: ISymbol) (derivation:GrammarToken list) (g: Grammar) = 
    let rule = Rule(g)
    let builder = rule
                    .GetBuilder()
                    .SetToken(NonTerminal(symbol, -1))
    derivation |> List.iter (
        fun t -> 
                builder.Add(t) 
                |> ignore
        )
    g.GetBuilder().Add(rule) |> ignore
    rule

/// Add multiple rules with derivations to the grammar
let internal add_rules_to_grammar (symbol: ISymbol) (derivations:GrammarToken list list) (g: Grammar) = 
    let add_symbol_rule_to_grammar = add_rule_to_grammar symbol
    derivations 
    |> List.iter (fun d -> g |> add_symbol_rule_to_grammar d |> ignore)
    g

/// Add a non-terminal to the derivation of a rule
let internal add_non_terminal_to_derivation (symbol: ISymbol) (rule: Rule) : Rule =
    rule.GetBuilder().Add(NonTerminal(symbol, -1)) |> ignore
    rule

/// Expand the derivation of a rule with a list of tokens
let internal expand_rule_derivation (tokens: GrammarToken list) (rule: Rule) : Rule =
    tokens 
    |> List.map (fun t -> rule.GetBuilder().Add(t)) 
    |> ignore
    rule

/// Get a new derivation by removing the first token from a given derivation
let internal remove_first_token (r: Rule) =
    let derivation = r.Derivation |> List.ofSeq
    match derivation with
        | _::tail -> tail
        | _ -> []

/// Remove a rule from the grammar
let internal remove_rule_from_grammar (rule: Rule) (g: Grammar) =
    g.GetBuilder().Remove(rule).Build()

/// Remove a list of rules from the grammar
let internal remove_rules_from_grammar (rules: Rule list) (g: Grammar) =
    let builder = g.GetBuilder()
    rules 
    |> List.iter (fun r -> builder.Remove(r) |> ignore)
    g

(*- - - - - OPERATIONS ON REGULAR EXPRESSIONS - - - - -*)

/// Add a regular expression to the grammar
let internal add_regex_to_grammar (symbol: ISymbol) (pattern: string) (g: Grammar) =
    let regex = RegularExpression(g)
                    .GetBuilder()
                    .SetToken(NonTerminal(symbol, -1))
                    .AddSymbols(pattern)
                    .Build()
    g.GetBuilder().Add(regex) |> ignore
    regex

/// Remove a regular expression from the grammar
let internal remove_regex_from_grammar (regex: RegularExpression) (g: Grammar) =
    g.GetBuilder().Remove(regex) |> ignore
    g

/// Remove a list of regular expressions from the grammar
let internal remove_regexs_from_grammar (regexs: RegularExpression list) (g: Grammar) =
    let builder = g.GetBuilder()
    regexs 
    |> List.iter (fun r -> builder.Remove(r) |> ignore)
    g