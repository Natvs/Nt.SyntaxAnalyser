module Nt.SyntaxAnalyser.LLParsing.LL1Parser

open Nt.SyntaxParser.Syntax.Structures
open Nt.SyntaxAnalyser.LLParsing.Derecursivation
open Nt.SyntaxAnalyser.LLParsing.Factorisation

[<CompiledName("Parse")>]
let parse(g: Grammar): Grammar =
    g
    |> eliminate_recursivity
    |> factorise