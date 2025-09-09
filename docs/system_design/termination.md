---
uid: termination
---
# Terminace

Terminace nastane v cyklu algoritmu pokud jedno ze zadaných terminačních pravidel vrátí `true`. Potom algoritmus skončí a vrátí nalezené řešení.

Terminačních pravidel může být více najednou a stačí, aby pouze jedno z nich vrátilo `true`.

## Terminační pravidla

### Fixovaný počet iterací

Toto pravidlo vrátí `true`, pokud aktuální počet iterací dosáhne hodnoty [iteration_count](~/user_guide/config_file.md#iteration_count).

### V procentuální blízkosti zatím nejlepšího řešení

Pokud je v aktuální iteraci nalezeno řešení, které je kratší než o [ceiling_percentage](~/user_guide/config_file.md#ceiling_percentage) procent delší řešení než zatím nalezené, tak se zvýší počet těchto řešení.

Počet se vynuluje pokud je nalezeno nové zatím nejlepší řešení.

Pokud počet dosáhne [in_row_termination_count](~/user_guide/config_file.md#in_row_termination_count), tak pravidlo vrátí `true`.