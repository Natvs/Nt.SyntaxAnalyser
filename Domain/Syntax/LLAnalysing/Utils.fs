module Nt.Syntax.LLAnalysing.Utils

open Nt.Parser.Symbols
open Nt.Syntax.Structures

// Syntax symbols

let rec private compare_chars (chars1: char list) (chars2: char list) =
    match chars1, chars2 with
    | [], [] -> 0
    | h1::t1, h2::t2 when h1 = h2 -> compare_chars t1 t2
    | h1::_, h2::_ -> if h1 < h2 then 1 else -1
    | [], _::_ -> -1
    | _, [] -> 1

let private compare_strings (str1: string) (str2: string) =
    (str1.ToCharArray() |> List.ofSeq, 
    str2.ToCharArray() |> List.ofSeq)
    ||> compare_chars

type SyntaxSymbol(name: string) =
    interface ISymbol with
        member _.get_Name() = name

    interface System.IComparable with
        member _.CompareTo(obj: obj) =
            if obj = null then 1
            else
                match obj with
                | :? SyntaxSymbol as o ->
                    let osymbol = o :> ISymbol
                    if name = osymbol.Name then 0 
                    else compare_strings name osymbol.Name
                | _ -> invalidArg "obj" "Object is not a HashSymbol"

    override _.Equals(obj: obj) =
        match obj with
        | :? SyntaxSymbol as o when name = (o :> ISymbol).Name -> true
        | _ -> false
    override _.GetHashCode() = hash name
    override _.ToString() = name

type SyntaxSymbolFactory() =
    interface ISymbolFactory with
        member _.Create(name: string) = SyntaxSymbol(name)

/// Get an ordered list of syntax symbols
let internal get_ordered_symbols (symbols: SyntaxSymbol list) =
    let rec fusion (l1: SyntaxSymbol list) (l2: SyntaxSymbol list) =
        match l1, l2 with
        | [], l | l, [] -> l
        | h1::t1, h2::_ when (h1 :> ISymbol).Name < (h2 :> ISymbol).Name -> h1::(fusion t1 l2)
        | _, h2::t2 -> h2::(fusion l1 t2)
    
    let rec divide (l: SyntaxSymbol list) =
        match l with
        | [] -> []
        | [x] -> [x]
        | _ ->
            let half = l.Length / 2
            let left = l.[..half - 1]
            let right = l.[half..]
            fusion (divide left) (divide right)

    divide symbols

/// Compare two sets for equality
let rec internal compare_sets (set1: Set<'a> ) (set2: Set<'a> ) =
    set1.Count = set2.Count && set1.IsSubsetOf(set2)

/// Compare two lists for equality
let rec internal compare_lists (list1: 'a list) (list2: 'a list) =
    match (list1, list2) with
    | ([], []) -> true
    | (h1::t1, h2::t2) when h1 = h2 -> compare_lists t1 t2
    | _ -> false

/// Compare two maps of HastSymbol for equality
let rec internal compare_maps (map1: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (map2: Map<SyntaxSymbol, Set<SyntaxSymbol>>) =
    let rec _compare_maps (map1: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (map2: Map<SyntaxSymbol, Set<SyntaxSymbol>>) =
        match map1.Keys |> List.ofSeq with
        | [] -> true
        | key::_ when map2.ContainsKey key ->
            map1[key].Count = map2[key].Count &&
            compare_sets (map1[key] |> Set.ofSeq) (map2[key] |> Set.ofSeq) &&
            compare_maps (map1.Remove(key)) (map2.Remove(key))
        | _ -> false

    map1.Keys.Count = map2.Keys.Count && _compare_maps map1 map2

/// Check if a sequence of grammar tokens can generate the empty string
let rec internal is_sequence_empty_generator (empty_generators: ISymbol list) (sequence: GrammarToken list) =
    match sequence with
    | [] -> true
    | token::_ when token.Type = GrammarTokenType.Terminal -> false
    | token::tail when empty_generators |> List.contains token.Symbol -> tail |> is_sequence_empty_generator empty_generators
    | _ -> false

/// Get the firsts symbols of a sequence of grammar tokens
let rec internal get_sequence_firsts (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (sequence: GrammarToken list) =
    match sequence with
    | [] -> Set.empty
    | token::_ when token.Type = GrammarTokenType.Terminal ->
        Set.singleton (token.Symbol :?> SyntaxSymbol)
    | token::tail when List.contains token.Symbol empty_generators ->
        match firsts.TryFind(token.Symbol :?> SyntaxSymbol) with
        | Some lst -> lst + (get_sequence_firsts empty_generators firsts tail)
        | None -> get_sequence_firsts empty_generators firsts tail
    | token::_ ->
        match firsts.TryFind(token.Symbol :?> SyntaxSymbol) with
        | Some lst -> lst
        | None -> Set.empty


