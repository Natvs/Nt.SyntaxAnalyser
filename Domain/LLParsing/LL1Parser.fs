module Nt.Syntax.LLParsing.LL1Parser

open Nt.SyntaxParser.Syntax.Structures
open Nt.Syntax.LLParsing.Derecursivation
open Nt.Syntax.LLParsing.Factorisation

[<CompiledName("Parse")>]
let parse(g: Grammar): Grammar =
    g
    |> eliminate_recursivity
    |> factorise