# Pretreating the grammar
In order to make the grammar LL(1) compliant, some pretreatments steps are applied to the grammar before using it to produce a syntax tree. In order, these are
1. Rules factorisation
2. Derecursivation
3. Regular expressions factorisation
4. Redundant rules removal

## Rules factorisation
Factorisation consists in extracting common prefixes from the productions of a same non-terminal. 

**Elimination of common prefixes**

For example, given the following rules 

`A -> a B | a C | d`

the resulting rules would be 

`A -> a A' | d` and `A' -> B | C`.

## Derecursivation
Derecursivation consists in removing left recursion from the grammar. These recursions can be direct or indirect.

**Elimination of left recursivity:**

Given a non-terminal A that has direct left recursivity, with rules of the form

`A -> A a | b`

the resulting rules would be

`A -> b A'` and `A' -> a A' | `.

To ensure that indirect recursivity is also removed, the derecursivation algorithm processes the non-terminals in a specific order, replacing rules that could lead to indirect left recursivity before removing direct left recursivity.

For example, given the following rules

`A -> B a | c` and `B -> A b | d`

These rules would first be transformed into

`A -> B a | c` and `B -> B a b | c a b | d`

before removing the direct left recursivity of B, leading to the final rules

`A -> B a | c` , `B -> a b B' | c a b B' | d B'` and `B' -> a b B' | `.

> Note: a rule like `A' -> ` means that A' produces the empty string.

## Regular expressions factorisation
This step has is divided into two sub-steps:
1. Multiple regular expressions defined for a same symbol are merged together
2. Regular expressions for rules that have the same prefix are also merged together

The first step is straightforward. For example, if a symbol `digit` has the following regular expressions defined `digit = [0-3]` and `digit = [4-9]`, then the merged regular expression would be `digit = [0-3]|[4-9]`.

The second step is a bit more complex, looking for regular expressions in any rules, to find those that have common prefixes and merge them together. Let's look at an example.

```
A -> var IDENT = INT
A -> var IDENT = STRING
A -> var IDENT = CHAR

IDENT = [a-zA-Z_][a-zA-Z0-9_]*
INT = [1-9][0-9]*|0
STRING = "(^")*"
CHAR = '[^']'
```

In this case, the three rules for A have the same prefix `var IDENT = `, so the resulting rules would be

```
A -> var IDENT = INT_STRING_CHAR

INT_STRING_CHAR = [1-9][0-9]*|0|"(^")*"|'[^']'
```


