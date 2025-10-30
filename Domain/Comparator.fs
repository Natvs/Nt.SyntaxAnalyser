module Nt.Syntax.Comparator

open Nt.Parsing.Structures
open Nt.Syntax.Structures

[<CompiledName("CompareTokens")>]
let public compare_tokens (t1: Token) (t2: Token) = t1.Name = t2.Name

[<CompiledName("CompareTokens")>]
let rec public compare_tokens_list (l1: Token list) (l2: Token list) =
    match l1, l2 with
        | [], [] -> true
        | a::ta, b::tb when compare_tokens a b -> compare_tokens_list ta tb
        | _ -> false

[<CompiledName("CompareTokens")>]
let public compare_grammar_token (t1: GrammarToken) (t2: GrammarToken) =
    t1.Index = t2.Index && t1.Type = t2.Type

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
    match compare_grammar_token e1.Token e2.Token with
        | false -> false
        | true -> e1.Pattern = e2.Pattern

[<CompiledName("CompareRegExps")>]
let rec public compare_regexps_set (l1: RegularExpression list) (l2: RegularExpression list) =
    l1
    |> List.forall(fun e1 ->
        l2
        |> List.exists(fun e2 -> compare_regexps e1 e2)
    )

[<CompiledName("CompareGrammars")>]
let rec public compare_grammars (g1: Grammar) (g2: Grammar) =
    let comparisons = 
        (g1.Axiom = g2.Axiom)
        ::(compare_tokens_list (List.ofSeq g1.Terminals) (List.ofSeq g2.Terminals))
        ::(compare_tokens_list (List.ofSeq g1.NonTerminals) (List.ofSeq g2.NonTerminals))
        ::(compare_rules_set (List.ofSeq g1.Rules) (List.ofSeq g2.Rules))
        ::(compare_regexps_set (List.ofSeq g1.RegularExpressions) (List.ofSeq g2.RegularExpressions))
        ::[]
    comparisons |> List.forall (fun b -> b = true)

