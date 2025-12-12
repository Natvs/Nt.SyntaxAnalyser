module Nt.Syntax.LLAnalysing.Utils

open Nt.Syntax.Structures

/// Compare the lengths of two lists, where each element of the parent list is an integer list.
let rec internal compare_lists (old_firsts: int list list) (new_firsts: int list list) =
    match old_firsts, new_firsts with
    | [], [] -> true
    | old_head::old_tail, new_head::new_tail when old_head.Length = new_head.Length -> compare_lists old_tail new_tail
    | _ -> false


