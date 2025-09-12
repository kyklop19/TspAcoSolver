---
uid: ACS
---

# Ant colony system

## 1. Generace cest

Generaci cest zajišťuje @TspAcoSolver.AcsColony, která řídí @TspAcoSolver.AcsAnt, kteří hledající cesty. Na rozdíl of @AS se mravenci nevyskytují na více vláknech, protože je zvolena sekvenční verze algoritmu, kde mravenci staví na předchozích nalezených cestách a tedy mravenci jsou spouštěni vždy když předchozí mravenec nalezl cestu.

### Výběr hrany

Ve výběru hrany se nejdříve náhodně vybere jestli mravenec bude hranu vybírat způsobem spíše exploitačním či exploračním. Kde pravděpodobnost exploitačního způsobu je [`explo_proportion_const`](~/user_guide/config_file.md#explo_proportion_const) a exploračního je (1-[`explo_proportion_const`](~/user_guide/config_file.md#explo_proportion_const)).

#### Exploitační způsob

Další nenavštívený soused $n$ je vybrán dle následujícího vzorce:

$$
n = \argmax_{j\in \text{nenavštívení sousedé}}\{\tau_{ij}^{\alpha}\eta_{ij}^{\beta}\}
$$

- $i$ -- počáteční vrchol
- $j$ -- koncový vrchol/soused
- $\tau_{ij}$ -- množství feromonů na hraně $ij$
- $\eta_{ij}$ -- atraktivita hrany $ij$, kde $\eta_{ij} = \frac{1}{d_{ij}}$ ($d_{ij}$ -- délka hrany $ij$)
- $\alpha$ -- [`trail_level_factor`](~/user_guide/config_file.md#trail_level_factor)
- $\beta$ -- [`attractiveness_factor`](~/user_guide/config_file.md#attractiveness_factor)

#### Explorační způsob

Tento způsob je identický způsobu z [Ant system](ant_system.md#výběr-hrany)

### Lokální aktualizace feromonů

Navíc se feromony aktualizují po každém nalezení cesty, kde každá hrana $ij$, která je součástí nalezené cesty, je aktualizovaná dle následujícího vzorce:

$$
\tau_{ij} = \max\{(1-\varphi)\cdot \tau_{ij} + \varphi \cdot \tau_0,\tau_{\min}\}
$$

- $i$ -- počáteční vrchol
- $j$ -- koncový vrchol/soused
- $\tau_{ij}$ -- množství feromonů na hraně $ij$
- $\varphi$ -- [`decay_coef`](~/user_guide/config_file.md#decay_coef)
- $\tau_0$ -- [`initial_pheromone_amount`](~/user_guide/config_file.md#initial_pheromone_amount)
- $\tau_{\min}$ -- minimální povolené množství feromonů na hraně

## 2. Zpracování cest

Pro globální aktualizaci je zvolena pouze nejkratší nalezená cesta.

## 3. Globální aktualizace feromonů

V tomto kroku je aktualizovaná nejkratší nalezená cesta a z celého grafu se vypařují feromony dle tohoto vzorce:

$$
\begin{align*}
\tau_{ij} &= \max\{(1-\rho)\cdot \tau_{ij} + \rho\Delta\tau_{ij},\tau_{\min}\}\\
\Delta\tau_{ij} &= \begin{cases}
\frac{Q}{L} \text{ pokud hrana } ij \text{ je součástí nejkratší nalezené cesty}\\
0 \text{ jinak}
\end{cases}
\end{align*}
$$

- $i$ -- počáteční vrchol
- $j$ -- koncový vrchol/soused
- $\tau_{ij}$ -- množství feromonů na hraně $ij$
- $\rho$ -- [`evaporation_coef`](~/user_guide/config_file.md#evaporation_coef)
- $\tau_{\min}$ -- minimální povolené množství feromonů na hraně
- $Q$ -- [`pheromone_amount`](~/user_guide/config_file.md#pheromone_amount)
- $L$ -- délka nejkratší nalezené cesty