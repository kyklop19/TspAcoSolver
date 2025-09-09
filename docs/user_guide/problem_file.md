# Soubory s problémem

Problém obchodního cestující je možné zadat v několika formátech souboru a to několika různými typy zadání.

## Typy zadání

### Explicitní zadání hran

Enum
:   `Explicit`

Pro každou hranu jsou zadány počáteční a konečný vrchol a délka této hrany.

### Souřadnicemi měst

Pro každé město/vrchol jsou zadány, jejich souřadnice a v závislosti na typu
zadání je vybraný prostor, ve kterém jsou. Díky tomu jsou potom vypočítány délky
hran.

#### V Euklidovském prostoru

Enum
:   `Euclid`

Souřadnice jsou zadány v Euklidovském prostoru a dle počtu souřadnic je určena dimenze.