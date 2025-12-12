# Pretreating the grammar
In order to make the grammar LL(1) compliant, two pretreatments are applied to the grammar before using it to produce a syntax tree: factorisation and derecursivation.

### Factorisation
Factorisation consists in extracting common prefixes from the productions of a same non-terminal. For example, given the following rules 

`A -> a B | a C | d`

the resulting rules would be 

`A -> a A' | d` and `A' -> B | C`.

### Derecursivation
Derecursivation consists in removing left recursion from the grammar. These recursions can be direct or indirect. For example, given the following rules

`A -> A a | b`

the resulting rules would be

`A -> b A'` and `A' -> a A' | `.

> Note: a rule like `A' -> ` means that A' produces the empty string.