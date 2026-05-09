module Nt.Syntax.LLParsing.LL1Parser

open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils
open Nt.Syntax.LLParsing.Derecursivation
open Nt.Syntax.LLParsing.Factorisation
open Nt.Syntax.LLParsing.RegexMerge
open Nt.Syntax.LLParsing.RulesSimplification


/// Remove all unused and unreachable terminals, non-terminals, rules and regexs
let private eliminate_unused_items (g: Grammar) =
    let unused_non_terminals = 
        g
        |> get_used_symbols (fun t -> t.Type = GrammarTokenType.NonTerminal)
        |> get_complementary (g.NonTerminals.GetSymbols() |> List.ofSeq)

    let unused_terminals =
        g
        |> get_used_symbols (fun t -> t.Type = GrammarTokenType.Terminal)
        |> get_complementary (g.Terminals.GetSymbols() |> List.ofSeq)

    g
    |> eliminate_unused_rules (g.Rules |> List.ofSeq) unused_non_terminals
    |> eliminate_unused_regexs (g.RegularExpressions |> List.ofSeq) unused_non_terminals
    |> remove_symbols_from_grammar remove_non_terminal_from_grammar (unused_non_terminals |> List.map (_.Name))
    |> remove_symbols_from_grammar remove_terminal_from_grammar (unused_terminals |> List.map (_.Name))

/// Parses the grammar to make it LL(1) compliant
[<CompiledName("Parse")>]
let public parse(g: Grammar): Grammar =
    g
    |> eliminate_recursivity
    |> factorise
    |> merge_regexs
    |> eliminate_identical_rules
    |> replace_unique_defined_non_terminals
    |> eliminate_unused_items

