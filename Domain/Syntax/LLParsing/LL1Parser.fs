module Nt.Syntax.LLParsing.LL1Parser

open Nt.Syntax
open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Derecursivation
open Nt.Syntax.LLParsing.Factorisation
open Nt.Syntax.LLParsing.RegexMerge
open Nt.Syntax.LLParsing.RulesEdition

/// Parses the grammar to make it LL(1) compliant
[<CompiledName("Parse")>]
let parse(g: Grammar): Grammar =
    g
    |> eliminate_recursivity
    |> factorise
    |> merge_regexs
    |> eliminate_identical_rules
    |> replace_unique_defined_non_terminals
    |> eliminate_unused_rules (g.Rules |> List.ofSeq)