# Začínáme

## První kroky

Po spuštění programu můžeme provést vícero věcí, ale pro nejzákladnější použití nám stačí nejdříve nastavit problém, který budeme řešit.

Pokud nemáte vlastní soubor s problémem doporučuji navštívit nějakou databázi s ukázkovými problémy jako např. [TSPLIB](http://comopt.ifi.uni-heidelberg.de/software/TSPLIB95/) (nadále v příkladech budeme používat problém z této databáze).

Pro nastavení problému použijeme příkaz `prob`:

```bash
prob C:\cesta_k_problemu\a280.tsp
```

Po tom co máme problém nastavený už stačí jenom program nechat vyřešit ho:

```bash
solve
```

Program následně zahájí výpočet a bude problém nějakou dobu řešit. Následně se zobrazí nějaké nalezené řešení podobně jako níže v příkladu (ale z povahy metody řešení problému nebude stejné).

```
```

## Pokročilé použití

Ve výše uvedeném příkladu jsme nijak neovlivňovali jakým způsobem program řeší zadaný problém. Program sám vybral výchozí parametry řešení.

Pokud bychom jeho chování chtěli změnit musíme mít nejdříve vlastní soubor s parametry. Aby jste si takový soubor mohli vytvořit, tak se podívejte, zde: @conf_file

Pokud soubor již máme, tak provedeme stejné kroky jako dříve, ale před použitím příkazu `solve` musíme tento soubor vybrat (nezáleží jestli nastavujeme nejdříve problém nebo parametry). To uděláme pomocí příkazu `conf`:

```bash
conf C:\cesta_k_parametrum\conf.yaml
```

***

Pro přehled příkazů se podívejte, zde: @cmds