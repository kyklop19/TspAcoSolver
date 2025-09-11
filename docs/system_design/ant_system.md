---
uid: AS
---

# Ant system

## 1. Generace cest

Cesty jsou generované pomocí @TspAcoSolver.AsColony, která využívá @TspAcoSolver.AsAnt. Jelikož je generace jednotlivých cest multi-agentními mravenci nezávislá na ostatních, tak @TspAcoSolver.AsColony spouští mravence na více vláknech.

### Výběr hrany

Mravenci si vybírají další vrchol v cestě náhodným výběrem, kde pravděpodobnost výběru vrcholu je následující:

$$
p_{ij} = \frac{\tau_{ij}^{\alpha}\eta_{ij}^{\beta}}{\sum_{l\in\text{unvisited nbrs}}\tau_{il}^{\alpha}\eta_{il}^{\beta}}
$$

- $i$ -- počáteční vrchol
- $j$ -- koncový vrchol/soused
- $\tau_{ij}$ -- množství feromonů na hraně $ij$
- $\eta_{ij}$ -- atraktivita hrany $ij$, kde $\eta_{ij} = \frac{1}{d_{ij}}$ ($d_{ij}$ -- délka hrany $ij$)
- $\alpha$ -- [`trail_level_factor`](~/user_guide/config_file.md#trail_level_factor)
- $\beta$ -- [`attractiveness_factor`](~/user_guide/config_file.md#attractiveness_factor)

## 2. Zpracování cest