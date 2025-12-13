module Nt.Syntax.LLAnalysing.Utils

open Nt.Syntax.Structures

/// Compare the lengths of two lists, where each element of the parent list is an integer list.
let rec internal compare_lists (old_firsts: int list list) (new_firsts: int list list) =
    match old_firsts, new_firsts with
    | [], [] -> true
    | old_head::old_tail, new_head::new_tail when old_head.Length = new_head.Length -> compare_lists old_tail new_tail
    | _ -> false

let rec internal get_sequence_firsts (empty_generators: int list) (firsts: int list list) (sequence: GrammarToken list) =
    match sequence with
    | [] -> []
    | token::_ when token.Type = GrammarTokenType.Terminal -> token.Index::[]
    | token::tail when empty_generators |> List.contains token.Index -> firsts[token.Index]@(get_sequence_firsts empty_generators firsts tail) |> List.distinct
    | token::_ -> firsts[token.Index]


