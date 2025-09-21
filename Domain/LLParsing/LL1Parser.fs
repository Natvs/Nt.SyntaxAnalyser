namespace Nt.SyntaxAnalyser.LLParsing

open Nt.SyntaxParser.Parsing.Structures
open Nt.SyntaxParser.Syntax.Structures
open System.Collections.Generic

module LL1Parser =

    /// Creates an empty rule for a given symbol index
    let private create_empty_rule (terminals: TokensList) (nonTerminals: TokensList) (symbolIndex:int) : Rule =
        let rule = new Rule(terminals, nonTerminals)
        rule.SetToken(symbolIndex, -1)
        rule

    /// Creates a rule for a given symbol index and derivation
    let private create_rule (terminals: TokensList) (nonTerminals: TokensList) (symbolIndex:int) (derivation:Derivation) : Rule =
        let rule = new Rule(terminals, nonTerminals)
        rule.SetToken(symbolIndex, -1)
        List.ofSeq (derivation)
        |> List.map (fun (t:GrammarToken) -> 
            match t with
                | :? Terminal -> rule.AddTerminal(t.Index, -1) 
                | :? NonTerminal -> rule.AddNonTerminal(t.Index, -1)
                | _ -> ()
        ) |> ignore
        rule

    /// Adds a non-terminal to the derivation of a rule
    let private add_non_terminal_to_derivation (index:int) (rule: Rule) : Rule =
        rule.AddNonTerminal(index, -1)
        rule

    /// Adds a rule to the grammar
    let private add_rule_to_grammar (g: Grammar) (rule: Rule) =
        g.Rules.Add(rule)

    /// Extends the name of a token at a given index
    let extend_token (tokens: TokensList) (index: int) (stringExtension: string) : Token =
        new Token(tokens.Item(index).Name + stringExtension)

    /// Adds a token to a tokens list
    let add_to_tokens (tokens: TokensList) (token: Token) : Token =
        tokens.Add(token)
        token

    /// Eliminates direct left recursivity for a given non-terminal in the grammar
    let private eliminate_direct_recursivity (tokens: TokensList) (g:Grammar) (s:NonTerminal) =
        let rules = List.ofSeq(g.Rules)
        let safeRules = List.filter (fun (r: Rule) -> (r.Token.Index = s.Index) && (r.Derivation[0].Index <> s.Index) ) rules
        let conflictRules = List.filter (fun (r: Rule) -> (r.Token.Index = s.Index) && (r.Derivation[0].Index = s.Index)) rules
        if conflictRules.IsEmpty then ()
        
        extend_token tokens s.Index "_rec" |> add_to_tokens tokens |> add_to_tokens g.NonTerminals |> ignore
        let newindex = g.NonTerminals.Count - 1

        (*Constructs new rules with same symbol and derivation but newsymbol at the end*)
        safeRules
        |> List.map (fun (r: Rule) -> 
            g.Rules.Remove(r) |> ignore
            create_rule g.Terminals g.NonTerminals r.Token.Index r.Derivation 
            |> add_non_terminal_to_derivation newindex
            |> add_rule_to_grammar g
        ) |> ignore

        (*Constructs new rules with newsymbol and derivation removed from the first item (recursive one)*)
        conflictRules
        |> List.map( fun (r: Rule) -> 
            g.Rules.Remove(r) |> ignore
            create_rule g.Terminals g.NonTerminals newindex r.Derivation 
            |> add_non_terminal_to_derivation newindex
            |> add_rule_to_grammar g
        ) |> ignore

        (*Adds an empty rule with newsymbol*)
        create_empty_rule g.Terminals g.NonTerminals newindex
        |> add_rule_to_grammar g
