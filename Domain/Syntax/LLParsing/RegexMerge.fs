module Nt.Syntax.LLParsing.RegexMerge

open Nt.Parsing.Structures
open Nt.Syntax.Structures
open Nt.Syntax.LLParsing.Utils

exception public EmptyPattern
exception public EmptyName

let rec private merge_patterns (patterns: string list) =
    match patterns with
    | [] -> raise(EmptyPattern)
    | pattern::[] -> pattern
    | pattern::tail -> pattern + "|" + (tail |> merge_patterns)

let rec private merge_names (names: string list) =
    match names with
    | [] -> raise(EmptyName)
    | name::[] -> name
    | name::tail -> name + "_" + (tail |> merge_names)

let internal replace_derivation_start (index: int) (rule: Rule) =
    rule.Derivation.RemoveAt(0)
    rule.InsertNonTerminal(0, index, -1)

let rec private merge_same_symbol_regex (g: Grammar) (index: int) =
    let regexs =
        g.RegularExpressions
        |> List.ofSeq
        |> List.filter (fun rg -> rg.Token.Index = index)

    (*Computes the new pattern and adds the new rule to grammar*)
    regexs
    |> List.map (fun rg -> rg.Pattern)
    |> merge_patterns
    |> create_regex g.NonTerminals (index)
    |> add_regex_to_grammar g
    |> ignore

    (*Removes the old rules*)
    regexs
    |> remove_regexs_from_grammar g

let rec private merge_same_symbol_rules (g: Grammar) (index: int) =
    let rules = 
        g.Rules
        |> List.ofSeq
        |> List.filter (fun r -> r.Token.Index = index && r.Derivation.Count > 0)
        |> List.filter (fun r -> 
            r.Derivation[0].Type = GrammarTokenType.NonTerminal &&
            g.RegularExpressions
            |> List.ofSeq
            |> List.exists (fun rg -> rg.Token.Index = r.Derivation[0].Index)
        )
    let symbols =
        rules
        |> List.filter (fun r -> r.Derivation.Count > 0)
        |> List.map (fun r -> r.Derivation[0].Index)
    
    if (symbols.Length > 1) then
        (*Computes the new symbol name*)
        let new_name =
            symbols
            |> List.map (fun index -> g.NonTerminals[index].Name)
            |> merge_names

        if new_name |> is_symbol_existing g.NonTerminals = false then
            new_name
            |> create_symbol
            |> add_to_symbols g.NonTerminals
            |> ignore

            let regexs =
                g.RegularExpressions
                |> List.ofSeq
                |> List.filter (fun rg -> symbols |> List.contains rg.Token.Index)

            (*Computes the new patterns and adds the new rule to grammar*)
            regexs
            |> List.map (fun rg -> rg.Pattern)
            |> merge_patterns
            |> create_regex g.NonTerminals (g.NonTerminals.Count - 1)
            |> add_regex_to_grammar g
            |> ignore

        let symbol_index =
            g.NonTerminals
            |> List.ofSeq
            |> List.findIndex (fun nt -> nt.Name = new_name)

        rules
        |> List.iter (fun r -> r |> replace_derivation_start symbol_index)
    g

[<CompiledName("Merge")>]
let merge_regexs (g: Grammar) =
    (*Merges patterns when a regex has several definitions*)
    [0 .. g.NonTerminals.Count-1]
    |> List.filter (fun index ->
        g.RegularExpressions
        |> List.ofSeq
        |> List.exists (fun rg -> rg.Token.Index = index)
    )
    |> List.iter (fun index -> merge_same_symbol_regex g index |> ignore)

    (*Merges patterns when a non terminal has two rules starting with regexs*)
    [0 .. g.NonTerminals.Count - 1]
    |> List.iter (fun index -> merge_same_symbol_rules g index |> ignore)
    g

