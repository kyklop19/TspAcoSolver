---
uid: reinit
---

# Reinicializace

Reinicializace nastane, pokud alespoň jedno z povolených pravidel vrátí `true`. Pokud se tak stane feromony na všech hranách se nastaví na počáteční hodnotu [initial_pheromone_amount](~/user_guide/config_file.md#initial_pheromone_amount).

## Reinicializační pravidla

### Fixovaný počet iterací

Po každých [iter_increment](~/user_guide/config_file.md#iter_increment) iteracích se feromony reinicializují.

### Pravidlo stagnace

Pokud se v [stagnation_ceiling](~/user_guide/config_file.md#stagnation_ceiling) po sobě jdoucích iteracích nenajde nové zatím nejlepší řešení, tak se feromony reinicializují.