module Nt.Syntax.LLParsing.Factorisation

open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils

exception public EmptyPatternException of string
exception public PatternNotFoundException of string
exception public InvalidRulesException of string

/// Keeps only the smallest pattern from a range of patterns
let rec private keep_smallest_pattern (patterns: GrammarToken list list): GrammarToken list =
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
let rec private get_common_pattern(reference: GrammarToken list) (rule: GrammarToken list) =
    match reference, rule with
        | [a], [b] when a.Type = b.Type && a.Symbol = b.Symbol -> [a]
        | rule_head::rule_tail, ref_head::ref_tail when rule_head.Type = ref_head.Type && rule_head.Symbol = ref_head.Symbol -> rule_head::(get_common_pattern rule_tail ref_tail)
        | _ -> []

/// Returns true if the rule derivation shares a common starting pattern with one of the rules in list
let private has_common_pattern (rules: Rule list) (rule: Rule) =
    rules
    |> List.filter(fun r -> r.Derivation.Count > 0)
    |> List.map(fun r -> (r.Derivation[0].Symbol, r.Derivation[0].Type))
    |> List.contains((rule.Derivation[0].Symbol, rule.Derivation[0].Type))

/// Given a set of rules, returns true if at least one of the rules should be factorised
let private need_factorisation (rules: Rule list): bool =
    if rules.Length > 1 then
        rules
        |> List.filter(fun r -> r.Derivation.Count > 0)
        |> List.map(fun r -> r |> has_common_pattern (rules |> List.except (r::[])) )
        |> List.contains(true)
    else false


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
    |> List.filter(fun r -> r <> [])
    |> keep_smallest_pattern

/// Returns true if the rule derivation starts with the given pattern
let rec private starts_with (pattern: GrammarToken list) (derivaiton: GrammarToken list): bool =
    match pattern, derivaiton with
        | [], _ -> raise (EmptyPatternException "Cannot find a rule starting with an empty pattern")
        | [a], b::_ when a.Type = b.Type && a.Symbol = b.Symbol -> true
        | a::p_tail, b::r_tail when a.Type = b.Type && a.Symbol = b.Symbol -> starts_with p_tail r_tail
        | _ -> false

/// Gets the tokens following a pattern in a rule
let rec private get_next (pattern: GrammarToken list) (derivation: GrammarToken list): GrammarToken list =
    match pattern, derivation with
        | [], _ -> raise (EmptyPatternException "Cannot find a rule starting with an emtpy pattern")
        | [a], b::tail when a.Type = b.Type && a.Symbol = b.Symbol -> tail
        | a::p_tail, b::r_tail when a.Type = b.Type && a.Symbol = b.Symbol -> get_next p_tail r_tail
        | _ -> raise (PatternNotFoundException "No rules starting with the given pattern have been found")

/// Factorise a set of rules
[<CompiledName("Factorise")>]
let rec public factorise_rules (g: Grammar) (rules_subset: Rule list): Grammar =
    if rules_subset.Length = 0 then raise (InvalidRulesException "Cannot factorise an empty set of rules")
    
    let common_symbol = rules_subset[0].Token.Symbol
    if rules_subset |> List.forall(fun r -> r.Token.Symbol = common_symbol) = false then raise (InvalidRulesException "Rules must have the same symbol")
    
    let reference = rules_subset |> List.find(fun r -> r |> has_common_pattern (rules_subset |> List.except (r::[])))
    let common_pattern = rules_subset |> get_rules_common_pattern reference
    let factorised_rules = rules_subset |> List.filter(fun r -> 
        r.Derivation 
        |> List.ofSeq 
        |> starts_with common_pattern
    )

    (*Add the new non-terminal used as new rules symbol*)
    let new_symbol =
        reference.Token.Symbol
        |> get_extended_name "_fact" g.NonTerminals
        |> add_as_non_terminal g

    (*Creates the new factorisation rules*)
    let new_derivations = 
        factorised_rules
        |> List.map (fun r ->
            r.Derivation
            |> List.ofSeq
            |> get_next common_pattern
        )

    (*Remove the rules to factorise from grammar*)
    g |> remove_rules_from_grammar factorised_rules

    (*Add the new factorisation rules to the grammar*)
    |> add_rules_to_grammar new_symbol new_derivations

    (*Create the common pattern rule*)
    |> add_rule_to_grammar reference.Token.Symbol common_pattern
    |> add_non_terminal_to_derivation new_symbol
    |> ignore


    (*Factorise the rules created in this factorisation*)
    let filtered_new_rules =
        g.Rules
        |> List.ofSeq
        |> List.filter(fun r -> r.Token.Symbol = new_symbol)
    if filtered_new_rules |> need_factorisation 
        then 
            filtered_new_rules 
            |> factorise_rules g 
            |> ignore 
        else ()

    (*Perform factorisation again on the rules with the same symbol*)
    let filtered_rules =
        g.Rules
        |> List.ofSeq
        |> List.filter(fun r -> r.Token.Symbol = rules_subset.Head.Token.Symbol)

    match (filtered_rules |> need_factorisation) with
        | true -> filtered_rules |> factorise_rules g
        | false -> g

/// Factorises a grammar
[<CompiledName("Factorise")>]
let rec public factorise (g: Grammar): Grammar =
    g.Rules 
    |> List.ofSeq 
    |> List.groupBy(fun (r:Rule) -> r.Token.Symbol)
    |> List.filter(fun (_, l) -> need_factorisation l)
    |> List.iter(fun (_, r) -> r |> factorise_rules g |> ignore)
    g
