module Nt.Syntax.Comparator

open Nt.Parser.Symbols
open Nt.Syntax.Structures
open System.Text.RegularExpressions

[<CompiledName("CompareTokens")>]
let public compare_tokens (t1: ISymbol) (t2: ISymbol) = 
    t1.Name = t2.Name

[<CompiledName("CompareTokens")>]
let rec public compare_tokens_list (l1: ISymbol list) (l2: ISymbol list) =
    match l1, l2 with
        | [], [] -> true
        | a::ta, b::tb when compare_tokens a b -> compare_tokens_list ta tb
        | _ -> false

[<CompiledName("CompareTokens")>]
let public compare_grammar_token (t1: GrammarToken) (t2: GrammarToken) =
    t1.Name = t2.Name && t1.Type = t2.Type

[<CompiledName("CompareTokens")>]
let rec public compare_patterns (p1: GrammarToken list) (p2: GrammarToken list) =
    match p1, p2 with
        | [], [] -> true
        | a::ta, b::tb when  compare_grammar_token a b -> compare_patterns ta tb
        | _ -> false

[<CompiledName("CompareRules")>]
let rec public compare_rules (r1: Rule) (r2: Rule) =
    match compare_grammar_token r1.Token r2.Token with
        | false -> false
        | true -> compare_patterns (List.ofSeq r1.Derivation) (List.ofSeq r2.Derivation)

[<CompiledName("CompareRules")>]
let rec public compare_rules_set (l1: Rule list) (l2: Rule list) =
    l1
    |> List.forall(fun r1 -> 
        l2
        |> List.exists(fun r2 -> compare_rules r1 r2)
    )

[<CompiledName("CompareRegExps")>]
let rec public compare_regexps (e1: RegularExpression) (e2: RegularExpression) =
    let rec get_next (current: char list) (pattern: char list) (depth: int) =
        match depth, pattern with
        | _, [] -> current, []
        | 0, '|'::tail -> current, tail
        | _, '('::tail -> get_next (current@['(']) tail (depth + 1)
        | _, ')'::tail -> get_next (current@[')']) tail (depth - 1)
        | _, c::tail -> get_next (current@[c]) tail depth

    let rec split_regex (pattern: char list) (depth: int) =
        let sub_pattern, left = get_next [] pattern 0
        match left with
        | [] -> [sub_pattern]
        | _ -> sub_pattern::(split_regex left depth)

    let rec compare_sub_patterns (p1: char list list) (p2: char list list) =
        match p1, p2 with
        | [], [] -> true
        | a::ta, b::tb when (a = b) -> compare_sub_patterns ta tb
        | _ -> false

    let sub_patterns_1 = split_regex (e1.Pattern.ToCharArray() |> List.ofArray) 0
    let sub_patterns_2 = split_regex (e2.Pattern.ToCharArray() |> List.ofArray) 0
    compare_grammar_token e1.Token e2.Token && compare_sub_patterns sub_patterns_1 sub_patterns_2
    
[<CompiledName("CompareRegExps")>]
let rec public compare_regexps_set (l1: RegularExpression list) (l2: RegularExpression list) =
    l1
    |> List.forall(fun e1 ->
        l2
        |> List.exists(fun e2 -> compare_regexps e1 e2)
    )

[<CompiledName("CompareGrammars")>]
let rec public compare_grammars (g1: Grammar) (g2: Grammar) =
    let compare_axioms =
        if g1.Axiom = null && g2.Axiom = null then true
        elif g1.Axiom = null || g2.Axiom = null then false
        else g1.Axiom.Symbol = g2.Axiom.Symbol

    let comparisons = 
        compare_axioms
        ::(compare_tokens_list (g1.Terminals.GetSymbols() |> List.ofSeq) (g2.Terminals.GetSymbols() |> List.ofSeq))
        ::(compare_tokens_list (g1.NonTerminals.GetSymbols() |> List.ofSeq) (g2.NonTerminals.GetSymbols() |> List.ofSeq))
        ::(compare_rules_set (List.ofSeq g1.Rules) (List.ofSeq g2.Rules))
        ::(compare_regexps_set (List.ofSeq g1.RegularExpressions) (List.ofSeq g2.RegularExpressions))
        ::[]
    comparisons |> List.forall (fun b -> b = true)

