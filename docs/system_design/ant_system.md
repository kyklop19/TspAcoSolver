---
uid: AS
---

# Ant system

## 1. Generace cest

Cesty jsou generované pomocí @TspAcoSolver.AsColony, která využívá @TspAcoSolver.AsAnt. Jelikož je generace jednotlivých cest multi-agentními mravenci nezávislá na ostatních, tak @TspAcoSolver.AsColony spouští mravence na více vláknech.

### Výběr hrany

Mravenci si vybírají další vrchol v cestě náhodným výběrem, kde pravděpodobnost výběru vrcholu je následující:

$$
p_{ij} = \frac{\tau_{ij}^{\alpha}\eta_{ij}^{\beta}}{\sum_{l\in\text{nenavštívení sousedé}}\tau_{il}^{\alpha}\eta_{il}^{\beta}}
$$

- $i$ -- počáteční vrchol
- $j$ -- koncový vrchol/soused
- $\tau_{ij}$ -- množství feromonů na hraně $ij$
- $\eta_{ij}$ -- atraktivita hrany $ij$, kde $\eta_{ij} = \frac{1}{d_{ij}}$ ($d_{ij}$ -- délka hrany $ij$)
- $\alpha$ -- [`trail_level_factor`](~/user_guide/config_file.md#trail_level_factor)
- $\beta$ -- [`attractiveness_factor`](~/user_guide/config_file.md#attractiveness_factor)

## 2. Zpracování cest

Při zpracování cest jsou všechny cesty při filtrování vráceny a tudíž se na všech aktualizují feromony.

## 3. Globální aktualizace feromonů

Aktualizují se všechny nalezené cesty a v celém grafu se vypařují feromony dle tohoto vzorce:

$$
\begin{align*}
\tau_{ij} &= \max\{(1-\rho)\cdot\tau_{ij} + \sum_{k\in \text{mravenci}}\Delta\tau_{ij}^{k},\tau_{\min}\}\\
\Delta\tau_{ij}^{k} &= \begin{cases}
\frac{Q}{L_k} \text{ pokud hrana } ij \text{ je součástí cesty mravence } k\\
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
- $L_k$ -- délka cesty nalezené mravencem $k$