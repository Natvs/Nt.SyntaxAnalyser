module Nt.Syntax.LLParsing.RulesSimplification

open Nt.Parser.Symbols
open Nt.Syntax
open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils

(* Removes the rules that are defined two times *)

let rec private eliminate_double_rules (rules: Rule list) (seen: string list) (g: Grammar) =
    match rules with
    | [] -> g
    | rule::tail when seen |> List.contains (rule.ToString()) ->
        g 
        |> remove_rules_from_grammar (rule::[])
        |> eliminate_double_rules tail seen
    | rule::tail -> g |> eliminate_double_rules tail ((rule.ToString())::seen)

[<CompiledName("RemoveDoubleRules")>]
let public eliminate_identical_rules (g: Grammar) = 
    g |> eliminate_double_rules (g.Rules |> List.ofSeq) []

(* Remove the rules that are unreachable *)

let rec private get_pattern_used_non_terminals (tokens: GrammarToken list) =
    match tokens with
    | [] -> []
    | token::tail when token.Type = GrammarTokenType.NonTerminal -> (token.Symbol)::(tail |> get_pattern_used_non_terminals)
    | _::tail -> tail |> get_pattern_used_non_terminals

let rec private get_used_non_terminals (rules: Rule list) =
    match rules with
    | [] -> []
    | rule::tail ->
        let used =
            rule.Derivation
            |> List.ofSeq
            |> get_pattern_used_non_terminals
        used@(tail |> get_used_non_terminals)

let internal get_unused_non_terminals (rules: Rule list) (g: Grammar) =
    let used = g.Axiom.Symbol::(rules |> get_used_non_terminals) |> List.distinct
    g.NonTerminals.GetSymbols() |> List.ofSeq |> List.except used

[<CompiledName("RemoveUnreachableRules")>]
let rec public eliminate_unused_rules (rules: Rule list) (unused: ISymbol list) (g: Grammar) =
    rules |> List.iter (fun r ->
            if unused |> List.contains r.Token.Symbol
            then g |> remove_rules_from_grammar (r::[]) |> ignore
        )

    let new_rules = g.Rules |> List.ofSeq
    if Comparator.compare_rules_set new_rules rules = false
    then g |> eliminate_unused_rules new_rules unused |> ignore
    g

(* Replacing in derivations the non-terminals that have only one definition *)

let rec private get_unique_defined_non_terminals (seen: ISymbol list) (left: ISymbol list) (rules: Rule list) =
    match rules with
    | [] -> left
    | rule::tail when seen |> List.contains rule.Token.Symbol -> 
        tail |> get_unique_defined_non_terminals seen (left |> List.except (rule.Token.Symbol::[]))
    | rule::tail -> tail |> get_unique_defined_non_terminals (rule.Token.Symbol::seen) left

let rec private get_new_pattern (rules: Rule list) (unique: ISymbol list) (pattern: GrammarToken list) =
    match pattern with
    | [] -> []
    | token::tail when token.Type = GrammarTokenType.NonTerminal && unique |> List.contains token.Symbol ->
        let insert_sequence = (rules |> List.find (fun r -> r.Token.Symbol = token.Symbol)).Derivation |> List.ofSeq
        insert_sequence@(tail |> get_new_pattern rules unique)
    | token::tail -> token::(tail |> get_new_pattern rules unique)
    
let rec private replace_non_terminals (unique: ISymbol list) (rules: Rule list) (g: Grammar) =
    match rules with
    | [] -> g
    | rule::tail ->
        let new_pattern = rule.Derivation
                            |> List.ofSeq
                            |> get_new_pattern (g.Rules |> List.ofSeq) unique
        g
        |> add_rule_to_grammar rule.Token.Symbol new_pattern
        |> ignore

        g
        |> remove_rule_from_grammar rule
        |> ignore

        (tail, g) ||> replace_non_terminals unique

[<CompiledName("MergeRules")>]
let rec public replace_unique_defined_non_terminals (g: Grammar) =
    let rules = g.Rules |> List.ofSeq
    let unique = 
        rules 
        |> get_unique_defined_non_terminals [] (g.NonTerminals.GetSymbols() |> List.ofSeq)
        |> List.filter (fun symbol -> rules |> List.exists (fun r -> r.Token.Symbol = symbol))
    1 |> ignore
    g |> replace_non_terminals unique rules |> ignore
    if Comparator.compare_rules_set (g.Rules |> List.ofSeq) rules = false then g |> replace_unique_defined_non_terminals |> ignore
    g