module Nt.Syntax.LLAnalysing.FollowsAnalyser

open Nt.Parser.Symbols;
open Nt.Syntax.Structures
open Nt.Syntax.LLAnalysing.Utils

/// Compute follows for a single derivation sequence
let rec private get_sequence_follows 
    (empty_generators: ISymbol list) 
    (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>)
    (follows: Map<SyntaxSymbol, Set<SyntaxSymbol>>) 
    (rule_symbol: ISymbol) 
    (derivation: GrammarToken list) =

    match derivation with
    | [] -> follows
    | token::tail when token.Type = GrammarTokenType.Terminal -> (rule_symbol, tail) ||> get_sequence_follows empty_generators firsts follows
    | token::tail when tail |> is_sequence_empty_generator empty_generators -> 
        let _get_new_follows_set (opt: Set<SyntaxSymbol> option) = 
            match opt with
            | Some lst -> match follows.TryFind(rule_symbol :?> SyntaxSymbol) with
                            | Some fv -> Some( lst + (tail |> get_sequence_firsts empty_generators firsts) + fv )
                            | None -> Some( lst + (tail |> get_sequence_firsts empty_generators firsts) )
            | None -> match follows.TryFind(rule_symbol :?> SyntaxSymbol) with
                            | Some fv -> Some( (tail |> get_sequence_firsts empty_generators firsts) + fv )
                            | None -> Some( tail |> get_sequence_firsts empty_generators firsts )
        let new_follows = follows.Change (token.Symbol :?> SyntaxSymbol, _get_new_follows_set)
        (rule_symbol, tail) ||> get_sequence_follows empty_generators firsts new_follows
    | token::tail -> 
        let _get_new_follows_set (opt: Set<SyntaxSymbol> option) = 
            match opt with
            | Some lst -> Some( lst + (tail |> get_sequence_firsts empty_generators firsts) )
            | None -> Some( tail |> get_sequence_firsts empty_generators firsts )
        let new_follows = follows.Change (token.Symbol :?> SyntaxSymbol, _get_new_follows_set)
        (rule_symbol, tail) ||> get_sequence_follows empty_generators firsts new_follows

/// Performs one iteration of computing follows for all rules
let rec private get_follows 
    (empty_generators: ISymbol list) 
    (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) 
    (follows: Map<SyntaxSymbol, Set<SyntaxSymbol>>) 
    (rules: Rule list) =

    match rules with
    | [] -> follows
    | rule::tail ->
        let new_follows = (rule.Token.Symbol, rule.Derivation |> List.ofSeq) ||> get_sequence_follows empty_generators firsts follows
        tail |> get_follows empty_generators firsts new_follows

/// Recursively compute follows until no changes occur
let rec private get_all_follows (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (follows: Map<SyntaxSymbol, Set<SyntaxSymbol>>) (rules: Rule list) =
    let new_follows = rules |> get_follows empty_generators firsts follows
    if (follows, new_follows) ||> compare_maps
        then new_follows
        else rules |> get_all_follows empty_generators firsts new_follows

/// Initialize the follows map with EOF for the axiom
let rec private init_follows (g: Grammar) =
    let eof = g.AddTerminal("EOF") :?> SyntaxSymbol
    let axiom = g.Axiom.Symbol :?> SyntaxSymbol
    Map [ (axiom, Set [ eof ]) ]

/// Public function to get the follows of a grammar. Adds EOF to the grammar if non existent.
[<CompiledName("Analyse")>]
let public analyse (g: Grammar) (empty_generators: ISymbol list) (firsts: Map<SyntaxSymbol, Set<SyntaxSymbol>>) =
    g.Rules
    |> List.ofSeq
    |> get_all_follows empty_generators firsts (g |> init_follows)