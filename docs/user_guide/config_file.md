---
uid: conf_file
---

# Konfigurační soubor

Konfigurační soubor slouží k nastavení parametrů pro výpočty programu. Tento soubor je ve formátu `.yaml` a uživatel může parametry pozměnit zadáním cesty k vlastnímu konfiguračnímu souboru pomocí příkazu [`conf`](commands.md#conf).

Vlastní konfigurační soubory nemusí obsahovat všechny parametry. Ty, které neobsahují a tedy nemění se jsou získány z výchozího konfiguračního souboru:

[!code-yaml[Výchozího konfiguračního soubor](~/../config/default_config.yaml)]

## Obsah

Většina parametrů je rozdělena do sekcí, kde jméno sekce je klíč a jeho hodnota
je mapa s parametry v sekci, tak jak je to ve výchozím konfiguračním souboru
výše.

### `algorithm`

Typ
:   `enum`

Tento parametr určuje jakým algoritmem se bude cesta v problému obchodního cestujícího hledat.

Implementačně ovlivňuje tento parametr jaká implementace @TspAcoSolver.ISolver, @TspAcoSolver.IColony a @TspAcoSolver.IAnt.

Hodnota | Algoritmus | Implementace
- | - | -
AS | @AS | Jsou použity implementace @TspAcoSolver.AsSolver, @TspAcoSolver.AsColony a @TspAcoSolver.AsAnt.
ACS | @ACS | Jsou použity implementace @TspAcoSolver.AcsSolver, @TspAcoSolver.AcsColony a @TspAcoSolver.AcsAnt.

### `pheromone_params`

Parametry ovlivňující inicializaci a aktualizaci feromonů v grafu.

#### `evaporation_coef`

Typ
:   `double` mezi 0 a 1

Algoritmus
:   @AS a @ACS

Ovlivňuje jaká část feromonů se při globální aktualizaci v @AS i @ACS "vypaří", tedy jak se hodnota feromonů změní. Více v informací u jednotlivých algoritmů.

#### `decay_coef`

Typ
:   `double` mezi 0 a 1

Algoritmus
:   @ACS

Ovlivňuje jaká část feromonů se při lokální aktualizaci v @AS "rozloží", tedy jak se hodnota feromonů změní. Více v informací v @AS.

#### `pheromone_amount`

Typ
:   `double`

Algoritmus
:   @AS a @ACS

Určuje jaké množství feromonů se uloží do grafu při globální aktualizaci v @AS a @ACS.

#### `calculate_initial_pheromone_amount`

Typ
:   `bool`

Algoritmus
:   @AS a @ACS

Pokud je `true`, tak je `initial_pheromone_amount` vypočítána automaticky dle následujícího vzorce:

$$
\tau_0 = \frac{1}{n \cdot L_{nn}}
$$

- $n$ -- počet měst v problému obchodního cestujícího
- $L_{nn}$ -- délka cesty získána @TspAcoSolver.NearestNbrAnt pomocí heuristiky nejbližšího souseda, tedy že další vrchol je vybrán ten nejbližší.

#### `initial_pheromone_amount`

Typ
:   `double`

Algoritmus
:   @AS a @ACS

Určuje jaké množství feromonů je na začátku (případně při @reinit) uloženo na každé hraně.

Pokud je [`calculate_initial_pheromone_amount`](#calculate_initial_pheromone_amount) `true`, tak nemá žádný efekt.

### `termination_params`
#### `termination_rule`

Typ
:   `flag`

Tento parametr určuje za jaké podmínky je vyhledávání cesty ukončeno.

Hodnota | Pravidlo
- | -
fixed |
within_percentage |

#### `iteration_count`

Typ
:   `int`

#### `ceiling_percentage`

Typ
:   `double` mezi 0 a 100

#### `in_row_termination_count`

Typ
:   `int`

### `reinitialization_params`
#### `reinitialization_rule`

Typ
:   `flag`

Tento parametr určuje za jaké podmínky jsou feromony grafu [reinicializovány](~/system_design/reinitialization.md).

Hodnota | Pravidlo
- | -
`fixed` | [Fixovaný počet iterací](~/system_design/reinitialization.md#fixovaný-počet-iterací)
`stagnation` | [Pravidlo stagnace](~/system_design/reinitialization.md#pravidlo-stagnace)

#### `iter_increment`

Typ
:   `int`

Reinicializační pravidlo
:   `fixed`

#### `stagnation_ceiling`

Typ
:   `int`

Reinicializační pravidlo
:   `stagnation`

### `colony_params`
#### `ant_count`

Typ
:   `int`

Tento parametr určuje kolik implementací @TspAcoSolver.IAnt v algoritmu hledá v jedné iteraci cestu, tedy kolik cest se v jedné iteraci maximálně vygeneruje.

#### `thread_count`

Typ
:   `int`

Algoritmus
:   @AS

Určuje kolik je maximálně použito vláken pro vygenerování cest v jedné iteraci.

#### `trail_level_factor`

Typ
:   `double`

Algoritmus
:   - @AS
    - @ACS

#### `attractiveness_factor`

Typ
:   `double`

Algoritmus
:   - @AS
    - @ACS

#### `explo_proportion_const`

Typ
:   `double`

Algoritmus
:   @ACS