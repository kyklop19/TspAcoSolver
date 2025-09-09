# Algoritmy

Pro řešení zadaného problému je možný výběr z několika algoritmů z rodiny **Ant colony optimization**.

## Obecný postup algoritmu

[!code-csharp[](~/../TspAcoSolver/Solver.cs#AlgorithmCore)]

Obecný postup algoritmů z této rodiny se odehrává ve `while` cyklu, který se ukončí po dosažení nějakého [terminačního pravidla](termination.md).

Kroky v cyklu jsou následující:

### 1. Generace cest

Nejprve se dle aktuálního stavu grafu problému obchodního cestujícího vytvoří nějaké množství cest v grafu.

Tento krok řídí nějaká implementace @TspAcoSolver.IColony dle algoritmu. Ta řídí určitý [počet mravenců](~/user_guide/config_file.md#ant_count), kde každý mravenec najde nějaké řešení, která (pokud jsou validní) vrátí.

### 2. Zpracování cest

Při zpracování cest se děje hned několik věcí:
- hledá se nejkratší cesta v jedné iteraci
- kontroluje se jestli se našla lepší cesta než doposud nejlepší nalezená cesta
- zvyšuje či resetuje se hodnota různých @TspAcoSolver.Counter
- filtrují se řešení, kterými budou následně aktualizovány feromony v grafu

### 3. Globální aktualizace feromonů

V tomto kroku se vezmou vyfiltrovaná řešení a hodnota feromonů na jejich hranách se zvedá dle algoritmu.

K tomu se na všech hranách "vypařují"/zmenšuje množství feromonů.

***

Nepovinným 4. krokem je poté @reinit.

## Implementované algoritmy

Algoritmus | Poznámky
- | -
@AS | Tento algoritmus je pravděpodobně tím nejzákladnějším ze své rodiny a je taky asi historicky prvním. Oproti ostatním je spíše méně efektivní, ale obecně ostatní algoritmy na něm většinou staví a jsou jeho rozšířením.
@ACS |