module Nt.SyntaxAnalyser.LLParsing.Factorisation

open Nt.SyntaxParser.Syntax.Structures
open Nt.SyntaxAnalyser.Utils

exception public EmptyPatternException of string
exception public PatternNotFoundException of string
exception public InvalidRulesException of string

/// Keeps only the smallest pattern from a range of patterns
let rec keep_smallest_pattern (patterns: GrammarToken list list): GrammarToken list =
    match patterns with
    | [] -> []
    | [p] -> p
    | p1::[p2] ->
        match p1.Length < p2.Length with
        | true -> p1
        | _ -> p2
    | p::tail -> 
        let tail_p = keep_smallest_pattern tail
        match p.Length < tail_p.Length with
        | true -> p
        | _ -> tail_p

/// Gets the common pattern of two list of tokens. The pattern is situated at the start of the list.
let rec get_common_pattern(reference: GrammarToken list) (rule: GrammarToken list): GrammarToken list =
    match reference, rule with
        | [a], [b] when a.Type = b.Type && a.Index = b.Index -> [a]
        | rule_head::rule_tail, ref_head::ref_tail  when rule_head.Type = ref_head.Type && rule_head.Index = ref_head.Index -> rule_head::(get_common_pattern rule_tail ref_tail)
        | _ -> []

/// Returns true if the rule derivation shares a common starting pattern with one of the rules in list
let has_common_pattern (rules: Rule list) (rule: Rule) : bool =
    rules
    |> List.map(fun r -> (r.Derivation[0].Index, r.Derivation[0].Type))
    |> List.contains((rule.Derivation[0].Index, rule.Derivation[0].Type))

/// Given a set of rules, returns true if at least one of the rules should be factorised
let need_factorisation (rules: Rule list): bool =
    rules
    |> List.map(has_common_pattern rules)
    |> List.contains(true)

/// For a given list of rules with the same symbol, returns the smallest common pattern shared with the reference
let rec private get_rules_common_pattern (reference: Rule) (rules : Rule list): GrammarToken list =
    /// Gets the common derivation pattern of two rules
    let common_rule_pattern (reference : Rule) (rule: Rule): GrammarToken list =
        let reference_derivation = 
            reference.Derivation
            |> List.ofSeq
        rule.Derivation 
        |> List.ofSeq 
        |> get_common_pattern reference_derivation

    rules 
    |> List.map(fun r -> common_rule_pattern reference r) 
    |> keep_smallest_pattern

/// Returns true if the rule derivation starts with the given pattern
let rec private starts_with (pattern: GrammarToken list) (derivaiton: GrammarToken list): bool =
    match pattern, derivaiton with
        | [], _ -> raise (EmptyPatternException "Cannot find a rule starting with an empty pattern")
        | [a], b::_ when a.Type = b.Type && a.Index = b.Index -> true
        | a::p_tail, b::r_tail when a.Type = b.Type && a.Index = b.Index -> starts_with p_tail r_tail
        | _ -> false

/// Gets the tokens following a pattern in a rule
let rec private get_next (pattern: GrammarToken list) (derivation: GrammarToken list): GrammarToken list =
    match pattern, derivation with
        | [], _ -> raise (EmptyPatternException "Cannot find a rule starting with an emtpy pattern")
        | [a], b::tail when a.Type = b.Type && a.Index = b.Index -> tail
        | a::p_tail, b::r_tail when a.Type = b.Type && a.Index = b.Index -> get_next p_tail r_tail
        | _ -> raise (PatternNotFoundException "No rules starting with the given pattern have been found")

/// Factorises a set of rules
[<CompiledName("Factorise")>]
let rec public factorise_rules (g: Grammar) (rules: Rule list): Grammar =
    let common_symbol = rules[0].Token.Index
    if rules |> List.forall(fun r -> r.Token.Index = common_symbol) = false then raise (InvalidRulesException "Rules should have the same symbol")
    if rules |> List.forall(fun r -> g.Rules.Contains(r)) = false then raise (InvalidRulesException "Rules should belong to the grammar")
    match rules |> need_factorisation with
        | false -> g
        | true -> 
            let reference = rules |> List.find(has_common_pattern rules)
            let common_pattern = rules |> get_rules_common_pattern reference
            let factorise_rules = rules |> List.filter(fun r -> 
                r.Derivation 
                |> List.ofSeq 
                |> starts_with common_pattern
            )

            (*Creates the new non-terminal used as new rules symbol*)
            "_fact"
            |> extend_token g.NonTerminals reference.Token.Index
            |> add_to_tokens g.NonTerminals
            |> ignore
            let new_index = g.NonTerminals.Count - 1

            (*Removes the rules to factorise from grammar*)
            factorise_rules 
            |> List.iter(fun r -> g.Rules.Remove r |> ignore)

            (*Creates the common pattern rule*)
            common_pattern
            |> create_rule g.Terminals g.NonTerminals reference.Token.Index
            |> add_non_terminal_to_derivation new_index
            |> add_rule_to_grammar g
            |> ignore

            (*Creates the new factorisation rules*)
            factorise_rules
            |> List.iter (fun r ->
                r.Derivation
                |> List.ofSeq
                |> get_next common_pattern
                |> create_rule g.Terminals g.NonTerminals new_index
                |> add_rule_to_grammar g
                |> ignore
            )
            g

[<CompiledName("Factorise")>]
/// Factorises a grammar
let rec public factorise (g: Grammar): Grammar =
    g.Rules 
    |> List.ofSeq 
    |> List.groupBy(fun (r:Rule) -> r.Token.Index)
    |> List.iter(fun (_, r) -> r |> factorise_rules g |> ignore)
    g
