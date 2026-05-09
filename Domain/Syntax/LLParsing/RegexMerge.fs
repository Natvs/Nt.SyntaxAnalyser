module Nt.Syntax.LLParsing.RegexMerge

open Nt.Parser.Symbols
open Nt.Syntax
open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils

exception public EmptyRegExPatternException
exception public EmptyRegExNameException

/// Get the merged regex patterns separated by '|'
let rec private merge_patterns (patterns: string list) =
    match patterns with
    | [] -> raise(EmptyRegExPatternException)
    | pattern::[] -> pattern
    | pattern::tail -> pattern + "|" + (tail |> merge_patterns)

/// Get the merged regex names separated by '_'
let rec private merge_names (names: string list) =
    match names with
    | [] -> raise(EmptyRegExNameException)
    | name::[] -> name
    | name::tail -> name + "_" + (tail |> merge_names)
    
/// Merge the regexs when a symbol has multiple regex definitions
let rec private merge_same_symbol_regex (g: Grammar) (symbol: ISymbol) =
    let regexs =
        g.RegularExpressions
        |> List.ofSeq
        |> List.filter (fun rg -> rg.Token.Symbol = symbol)

    (*Computes the new pattern*)
    let new_pattern = 
        regexs
        |> List.map (_.Pattern)
        |> merge_patterns

    (*Remove the old regexs and add the merged one to the grammar*)
    g
    |> remove_regexs_from_grammar regexs
    |> add_regex_to_grammar symbol new_pattern
    |> ignore

/// Merge the regexs when multiple rules start with the a regex symbol
let rec private merge_same_symbol_rules (g: Grammar) (symbol: ISymbol) =
    (*Computes rules with same symbol beginning by a regex symbol*)
    let rules = 
        g.Rules
        |> List.ofSeq
        |> List.filter (fun r -> r.Token.Symbol = symbol && r.Derivation.Count > 0)
        |> List.filter (fun r -> 
            r.Derivation[0].Type = GrammarTokenType.NonTerminal &&
            g.RegularExpressions
            |> List.ofSeq
            |> List.exists (fun rg -> rg.Token.Symbol = r.Derivation[0].Symbol)
        )

    (*Computes symbols of regex to merge*)
    let symbols =
        rules
        |> List.map (fun r -> r.Derivation[0].Symbol)
    
    if (symbols.Length > 1) then
        (*Computes the symbol name for the new regex*)
        let new_name =
            symbols
            |> List.map (fun s -> s.Name)
            |> merge_names

        if new_name |> is_symbol_existing (g.NonTerminals.GetSymbols() |> List.ofSeq) = false then
            let new_symbol = 
                new_name
                |> add_as_non_terminal g

            (*Add the new merged regex to the grammar*)
            let new_pattern =
                g.RegularExpressions
                |> List.ofSeq
                |> List.filter (fun rg -> symbols |> List.contains rg.Token.Symbol)
                |> List.map (_.Pattern)
                |> merge_patterns

            g
            |> add_regex_to_grammar new_symbol new_pattern
            |> ignore

        let new_token = NonTerminal(g.NonTerminals.Get(new_name), -1)

        (*Remove old rules and create new ones with the merged symbol*)
        rules
        |> List.iter (fun r -> 
            let derivation = r |> remove_first_token
            g
            |> remove_rule_from_grammar r
            |> add_rule_to_grammar r.Token.Symbol (new_token::[])
            |> expand_rule_derivation derivation
            |> ignore)
    g

/// Apply regexs merging on the grammar
[<CompiledName("Merge")>]
let merge_regexs (g: Grammar) =
    (*Two different merges happen here. 
    The first only merges the regular expressions when a symbol has multiple regex definitions.
    The second merges regular expressions when the regex symbol is the first symbol of derivation of rules with the same symbol.*)

    (*Merges patterns when a regex has several definitions*)
    g.NonTerminals.GetSymbols()
    |> List.ofSeq
    |> List.filter (fun nt ->
        g.RegularExpressions
        |> List.ofSeq
        |> List.exists (fun rg -> rg.Token.Symbol = nt)
    )
    |> List.iter (fun index -> merge_same_symbol_regex g index |> ignore)

    (*Merges patterns when a non terminal has two rules starting with regexs*)
    g.NonTerminals.GetSymbols()
    |> List.ofSeq
    |> List.iter (fun symbol -> merge_same_symbol_rules g symbol |> ignore)

    g

[<CompiledName("RemoveUnreachableRegexs")>]
let rec public eliminate_unused_regexs (regexs: RegularExpression list) (unused: ISymbol list) (g: Grammar) =
    regexs |> List.iter (fun r ->
            if unused |> List.contains r.Token.Symbol
            then g |> remove_regexs_from_grammar (r::[]) |> ignore
        )

    let new_regexs = g.RegularExpressions |> List.ofSeq
    if Comparator.compare_regexs_set new_regexs regexs = false
    then g |> eliminate_unused_regexs new_regexs unused |> ignore
    g

