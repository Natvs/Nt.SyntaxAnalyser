module Nt.Syntax.LLParsing.LL1Parser

open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Derecursivation
open Nt.Syntax.LLParsing.Factorisation
open Nt.Syntax.LLParsing.RegexMerge
open Nt.Syntax.LLParsing.Utils

/// Eliminate rules that are duplicates in the grammar
let rec private eliminate_double_rules (rules: Rule list) (seen: string list) (g: Grammar) =
    match rules with
    | [] -> g
    | rule::tail when seen |> List.contains (rule.ToString()) ->
        g
        |> remove_rule_from_grammar rule
        |> eliminate_double_rules tail seen
    | rule::tail -> g |> eliminate_double_rules tail ((rule.ToString())::seen)

/// Parses the grammar to make it LL(1) compliant
[<CompiledName("Parse")>]
let parse(g: Grammar): Grammar =
    g
    |> eliminate_recursivity
    |> factorise
    |> merge_regexs
    |> eliminate_double_rules (g.Rules |> List.ofSeq) []