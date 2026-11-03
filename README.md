# Provocarea 05B — Fabrica TM: a doua linie de ambalare

## 1. Răspunsurile la cele 4 întrebări (înainte de orice modificare)

**1. De ce `line1.Name` afișează „Linia 2"?**
În codul inițial `name` este `static`: există **o singură copie**, aparținând clasei, nu obiectului. `line1` și `line2` citesc același câmp. Constructorul lui `line2` scrie `name = "Linia 2"` peste „Linia 1", iar `line1.Name` citește acum aceeași valoare.

**2. De ce viteza liniei 1 se schimbă când operatorul setează viteza liniei 2?**
Același motiv: `speed` este `static`, deci o singură variabilă pentru toată fabrica. `line1.SetSpeed(110)` o face 110, apoi `line2.SetSpeed(100)` o suprascrie cu 100, iar `line1.Speed` (și `Run`) văd 100. Același lucru se întâmplă cu `packedCount`, `energyKwh` și `currentBatch`: de aceea pică și verificările 4, 5, 8, 11, 12. În plus, `MaxSpeed` era o constantă de 120 pentru **toate** liniile, deci PL-80 nu avea de unde să fie limitat la 80 (verificarea 3).

**3. De ce trec verificările 7 și 9? Ce se întâmplă dacă scot `static` de peste tot?**
Trec pentru că `nextBatchNumber` și `linesCreated` sunt *chiar* date ale fabricii, nu ale unei linii: un singur contor comun pentru coduri de lot și un singur contor de linii. „Static" era corect acolo; greșit era doar restul.
Dacă scot `static` de peste tot:
- `const` este implicit static, nu se poate scoate; `PlantCode` rămâne, deci problema nu e acolo.
- `PackagingLine.LinesCreated` și `PackagingLine.TotalPacked` ar deveni membri de instanță și `Program.cs` nu s-ar mai compila (`CS0120: An object reference is required...`; verificat).
- Dacă forțez compilarea (conturile de instanță, proprietățile statice înlocuite cu valori fixe), fiecare linie își are propriul `nextBatchNumber` începând de la 1: `b1` și `b2` ar fi ambele `TM-0001` (verificarea 7 pică), iar fiecare obiect ar avea `linesCreated == 1` (verificarea 9 pică). Verificat prin simulare: 7, 8 și 9 devin `BUG`.

Concluzie: nu „scot static peste tot" și nici „las static peste tot"; fiecare membru primește modificatorul potrivit după **cui aparține**.

**4. Tariful din `plant.config` este 0,97. De ce programul calcula cu 0,92?**
Pentru că în clasă era `public const double EnergyTariff = 0.92;`. `const` înseamnă o valoare literală fixată **la compilare** (compilatorul o și copiază în locurile unde e folosită). Codul nu citea niciodată fișierul: acesta era citit doar de verificarea din `Program.cs`, deci se comparau 0,92 și 0,97. Valoarea e cunoscută abia la execuție, deci nu poate fi `const`; trebuie să fie `static readonly`, atribuită într-un constructor static.

## 2. Diagrama de memorie (după crearea celor două linii și rularea scenariului)

```
 LA NIVELUL CLASEI PackagingLine  (o singură dată pentru toată fabrica)
 ┌───────────────────────────────────────────────────────────────────┐
 │ const PlantCode              = "TM"     (literal, fără loc propriu)│
 │ static readonly EnergyTariff = 0.97     (citit din plant.config,   │
 │                                          o dată, în ctor static)   │
 │ static nextBatchNumber       = 4        (contor comun de loturi)   │
 │ static linesCreated          = 2                                   │
 │ static totalPacked           = 9200                                │
 └───────────────────────────────────────────────────────────────────┘
        ▲                                   ▲
        │ (ambele obiecte folosesc aceleași date statice)
 ┌──────┴───────────────────┐       ┌────────┴─────────────────┐
 │ line1  (PL-120)          │       │ line2  (PL-80)           │
 │ MaxSpeed      = 120      │       │ MaxSpeed      = 80       │
 │ name          = "Linia 1"│       │ name          = "Linia 2"│
 │ speed         = 110      │       │ speed         = 80       │
 │ packedCount   = 4400     │       │ packedCount   = 4800     │
 │ energyKwh     = 5.0      │       │ energyKwh     = 6.0      │
 │ currentBatch  = "TM-0003"│       │ currentBatch  = "TM-0002"│
 └──────────────────────────┘       └──────────────────────────┘
      (copie proprie a liniei 1)         (copie proprie a liniei 2)
```

## 3. Clasificarea membrilor

| Membru | Al fabricii sau al liniei? | Se schimbă după creare? | Declarație aleasă | De ce |
|---|---|---|---|---|
| `PlantCode` | Fabrică | Nu, niciodată | `const` | Valoare cunoscută la compilare și imuabilă („TM"). |
| `EnergyTariff` | Fabrică | Nu după inițializare (se schimbă între rulări, prin fișier) | `static readonly` | Valoarea e cunoscută doar la execuție (din `plant.config`), identică pentru toate liniile, atribuită o dată în constructorul static. |
| `MaxSpeed` | Linie (depinde de model) | Nu | instanță, `readonly` (proprietate numai-get atribuită în constructor) | Fiecare linie are propria limită (120 / 80), primită la creare. |
| `name` | Linie | Nu | instanță, `readonly` | Fiecare linie are numele ei; nu se redenumește după creare. |
| `speed` | Linie | Da (`SetSpeed`) | instanță | Fiecare linie merge cu viteza ei. |
| `packedCount` | Linie | Da (`Run`) | instanță | Producția fiecărei linii, separat. |
| `energyKwh` | Linie | Da (`Run`) | instanță | Consumul fiecărei linii, separat. |
| `currentBatch` | Linie | Da (`StartBatch`) | instanță | Fiecare linie lucrează la lotul ei. |
| `nextBatchNumber` | Fabrică | Da (la fiecare lot) | `static` (privat) | Un singur contor ca să fie coduri unice în toată fabrica. |
| `linesCreated` | Fabrică | Da (la fiecare linie nouă) | `static` (privat) | Numără obiectele create; nu aparține niciunuia. |
| total pachete fabrică (`totalPacked`) | Fabrică | Da (la fiecare `Run`) | `static` (privat) | Suma peste toate liniile; `Run` îl crește odată cu `packedCount`-ul liniei. |

## 4. Ieșirea programului

```
OK   1. Linia 1 își păstrează numele
OK   2. Viteza liniei 1 este 110 pachete/min
OK   3. Viteza liniei 2 este limitată la 80 pachete/min (PL-80)
OK   4. Linia 1: 110 × 30 + 110 × 10 = 4400 pachete
OK   5. Linia 2: 80 × 60 = 4800 pachete
OK   6. Total fabrică: 9200 pachete
OK   7. Codurile loturilor sunt unice în fabrică
OK   8. Lotul curent: linia 1 → al treilea lot, linia 2 → al doilea
OK   9. S-au creat 2 linii
OK   10. Tariful folosit este cel din plant.config (0.97 lei/kWh)
OK   11. Energia liniei 1: 7,5 kW × 40 min = 5 kWh
OK   12. Costul energiei liniei 2: 6 kWh × tarif

Toate verificările au trecut.
```

Verificat și cu `plant.config` modificat (`EnergyTariff=1.35`): verificarea 10 trece fără să se atingă `PackagingLine.cs`. Valoarea `0.97` nu apare în `PackagingLine.cs`.

## 5. Ce s-a modificat

- `PackagingLine.cs`: câmpurile de stare au devenit de instanță, `MaxSpeed` vine din constructor, `TotalPacked` are contor static propriu, tariful e citit într-un constructor static (`CultureInfo.InvariantCulture`; ignoră liniile goale și comentariile `#`; fișierul e căutat lângă executabil, unde îl copiază `.csproj`).
- `Program.cs`: **numai** cele două linii `new PackagingLine(...)` (se adaugă viteza maximă: 120 și 80). Partea 2 este neatinsă (`git diff` o arată).

## 6. Când `const`, când `static readonly`

Aleg `const` când valoarea este cunoscută la scrierea codului și nu se schimbă niciodată (ea e copiată în program la compilare), și `static readonly` când valoarea e comună tuturor obiectelor, dar se află abia la execuție (fișier, calcul) sau poate fi schimbată fără recompilare, ca tariful.
