module Nt.Syntax.LLParsing.RulesEdition

open Nt.Syntax
open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils

(* Removes the rules that are defined two times *)

let rec private eliminate_double_rules (rules: Rule list) (seen: string list) (g: Grammar) =
    match rules with
    | [] -> g
    | rule::tail when seen |> List.contains (rule.ToString()) ->
        rule::[] 
        |> remove_rules_from_grammar g
        |> eliminate_double_rules tail seen
    | rule::tail -> g |> eliminate_double_rules tail ((rule.ToString())::seen)

[<CompiledName("RemoveDoubleRules")>]
let public eliminate_identical_rules (g: Grammar) = 
    g |> eliminate_double_rules (g.Rules |> List.ofSeq) []

(* Remove the rules that are unreachable *)

let rec private get_pattern_used_non_terminals (tokens: GrammarToken list) =
    match tokens with
    | [] -> []
    | token::tail when token.Type = GrammarTokenType.NonTerminal -> (token.Index)::(tail |> get_pattern_used_non_terminals)
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

[<CompiledName("RemoveUnreachableRules")>]
let rec public eliminate_unused_rules (rules: Rule list) (g: Grammar) =
    let used = g.Axiom::(rules |> get_used_non_terminals) |> List.distinct
    let unused = [0 .. g.NonTerminals.Count - 1] |> List.except used
    rules |> List.iter (fun r ->
        if unused |> List.contains r.Token.Index
        then r::[] |> remove_rules_from_grammar g |> ignore)

    let new_rules = g.Rules |> List.ofSeq
    if Comparator.compare_rules_set new_rules rules = false
    then (new_rules, g) ||> eliminate_unused_rules |> ignore
    g

(* Replacing in derivations the non-terminals that have only one definition *)

let rec private get_unique_defined_non_terminals (seen: int list) (left: int list) (rules: Rule list) =
    match rules with
    | [] -> left
    | rule::tail when seen |> List.contains rule.Token.Index -> 
        tail |> get_unique_defined_non_terminals seen (left |> List.except (rule.Token.Index::[]))
    | rule::tail -> tail |> get_unique_defined_non_terminals (rule.Token.Index::seen) left

let rec private get_new_pattern (rules: Rule list) (unique: int list) (pattern: GrammarToken list) =
    match pattern with
    | [] -> []
    | token::tail when token.Type = GrammarTokenType.NonTerminal && unique |> List.contains token.Index ->
        let insert_sequence = (rules |> List.find (fun r -> r.Token.Index = token.Index)).Derivation |> List.ofSeq
        insert_sequence@(tail |> get_new_pattern rules unique)
    | token::tail -> token::(tail |> get_new_pattern rules unique)
    
let rec private replace_non_terminals (unique: int list) (rules: Rule list) (g: Grammar) =
    match rules with
    | [] -> g
    | rule::tail ->
        rule.Derivation
        |> List.ofSeq
        |> get_new_pattern (g.Rules |> List.ofSeq) unique
        |> create_rule g.Terminals g.NonTerminals rule.Token.Index
        |> add_rule_to_grammar g
        |> ignore
        rule::[] |> remove_rules_from_grammar g |> ignore
        (tail, g) ||> replace_non_terminals unique

[<CompiledName("MergeRules")>]
let rec public replace_unique_defined_non_terminals (g: Grammar) =
    let rules = g.Rules |> List.ofSeq
    let unique = 
        rules 
        |> get_unique_defined_non_terminals [] [0 .. g.NonTerminals.Count - 1]
        |> List.filter (fun index -> rules |> List.exists (fun r -> r.Token.Index = index))
    1 |> ignore
    g |> replace_non_terminals unique rules |> ignore
    if Comparator.compare_rules_set (g.Rules |> List.ofSeq) rules = false then g |> replace_unique_defined_non_terminals |> ignore
    g