module Nt.Syntax.LLParsing.LL1Parser

open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Derecursivation
open Nt.Syntax.LLParsing.Factorisation
open Nt.Syntax.LLParsing.RegexMerge
open Nt.Syntax.LLParsing.RulesSimplification

let private eliminate_unused_rules_and_regexs (g: Grammar) =
    let unused_non_terminals = g |> get_unused_non_terminals (g.Rules |> List.ofSeq)

    g
    |> eliminate_unused_rules (g.Rules |> List.ofSeq) unused_non_terminals
    |> eliminate_unused_regexs (g.RegularExpressions |> List.ofSeq) unused_non_terminals

/// Parses the grammar to make it LL(1) compliant
[<CompiledName("Parse")>]
let public parse(g: Grammar): Grammar =
    g
    |> eliminate_recursivity
    |> factorise
    |> merge_regexs
    |> eliminate_identical_rules
    |> replace_unique_defined_non_terminals
    |> eliminate_unused_rules_and_regexs

