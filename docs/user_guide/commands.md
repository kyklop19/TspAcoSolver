---
uid: cmds
---

# Příkazy

## `prob`

```bash
prob <cesta-k-problemu> [--type/-t <type-name>] [-h]
```

Přečte problém ze souboru `<cesta-k-problemu>` a nastaví ho jako problém, který se bude řešit.

`--type/-t`
:   Určuje jaký [typ zadání](problem_file.md#typy-zadání) problému je v souboru. Je potřeba nastavit jen u některých formátů souboru ([`.csv`](csv_problem_file.md)).

## `conf`

```bash
conf <cesta-ke-konfiguracnimu-souboru> [-h]
```

Přečte parametry pro řešení ze souboru `<cesta-ke-konfiguracnimu-souboru>` a nastaví je.

Pro více informací o struktuře souboru nahlédněte zde: @conf_file

## `solve`

```bash
solve [--heatmap/-hm] [-h]
```

Začne řešit zadaný problém se zadanými parametry a po [terminaci](~/system_design/termination.md) vypíše nalezené řešení.

`--heatmap/-hm`
:   Při řešení se objeví okno vizualizující matici množství feromonů v grafu.

    ![heatmap](~/images/heatmap.png)

## `quit`

```bash
quit [-h]
```

Ukončí program.