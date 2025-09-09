# Formát `.csv`

Pro tento formát je nutné v příkazu [`prob`](commands.md#prob) zadat i typ zadání pro interpretaci dat.

## Typ zadání

### Explicitní zadání hran

V tomto typu zadání reprezentuje každý řádek jednu orientovanou hranu.

Každý řádek má tři sloupce.

- 1. sloupec je **počáteční** vrchol a tedy by měl být z množiny $\mathbb{N}_0$.
- 2. sloupec je **konečný** vrchol a tedy by měl být z množiny $\mathbb{N}_0$.
- 3. sloupec je **délka** hrany a může být jakékoliv reálné číslo.