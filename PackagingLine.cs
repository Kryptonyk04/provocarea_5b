// Linie de ambalare (cutii de carton) – fabrica TM.
//
// Fabrica are mai multe linii, de modele diferite:
//   PL-120: viteză maximă 120 pachete/min      PL-80: viteză maximă 80 pachete/min
// Putere consumată: 2 kW + 0,05 kW pentru fiecare pachet/min.
//
// Regula de proiectare: ce ține de UNA dintre linii este membru de instanță;
// ce ține de FABRICĂ (toate liniile la un loc) este static; ce nu se schimbă
// niciodată este const / readonly.
using System.Globalization;

public class PackagingLine
{
  // ---- Nivelul fabricii (clasei): o singură copie în toată aplicația ----

  public const string PlantCode = "TM";                 // constantă adevărată: codul fabricii

  // Tariful se citește o singură dată, în constructorul static, din plant.config.
  // Nu poate fi const (valoarea nu e cunoscută la compilare) și nu se schimbă
  // după inițializare => static readonly.
  public static readonly double EnergyTariff;           // lei/kWh

  private static int nextBatchNumber = 1;               // contor comun: coduri unice în fabrică
  private static int linesCreated;                      // câte linii au fost create
  private static int totalPacked;                       // total pachete, toate liniile

  // Se execută automat, o singură dată, înainte de prima folosire a clasei
  // (primul new PackagingLine sau primul acces la un membru static).
  static PackagingLine()
  {
    EnergyTariff = ReadTariff("plant.config");
  }

  public static int LinesCreated => linesCreated;
  public static int TotalPacked => totalPacked;

  // ---- Nivelul liniei (obiectului): câte o copie pentru fiecare linie ----

  public int MaxSpeed { get; }                          // depinde de model; fixat în constructor
  private readonly string name;                         // nu se schimbă după creare
  private int speed;                                    // pachete/min
  private int packedCount;
  private double energyKwh;
  private string currentBatch = "";

  public PackagingLine(string lineName, int maxSpeed)
  {
    name = lineName;
    MaxSpeed = maxSpeed;
    linesCreated++;
  }

  public string Name => name;
  public int Speed => speed;
  public int PackedCount => packedCount;
  public double EnergyKwh => energyKwh;
  public string CurrentBatch => currentBatch;

  public void SetSpeed(int packsPerMinute)
  {
    speed = Math.Clamp(packsPerMinute, 0, MaxSpeed);
  }

  // Fiecare lot primește un cod unic în toată fabrica: TM-0001, TM-0002, ...
  public string StartBatch()
  {
    currentBatch = $"{PlantCode}-{nextBatchNumber:D4}";
    nextBatchNumber++;
    return currentBatch;
  }

  public void Run(int minutes)
  {
    int packed = speed * minutes;
    packedCount += packed;
    totalPacked += packed;                              // și contorul fabricii
    double powerKw = 2 + 0.05 * speed;
    energyKwh += powerKw * minutes / 60.0;
  }

  public double EnergyCost()
  {
    return energyKwh * EnergyTariff;
  }

  // ---- Citirea configurației ----

  // Fișierul e copiat lângă executabil la build (vezi .csproj). Dacă nu e acolo,
  // se încearcă directorul curent.
  private static double ReadTariff(string fileName)
  {
    string path = Path.Combine(AppContext.BaseDirectory, fileName);
    if (!File.Exists(path))
      path = fileName;

    foreach (string rawLine in File.ReadAllLines(path))
    {
      string line = rawLine.Trim();
      if (line.Length == 0 || line.StartsWith('#'))
        continue;

      int eq = line.IndexOf('=');
      if (eq < 0 || line[..eq].Trim() != "EnergyTariff")
        continue;

      // InvariantCulture: punctul zecimal, indiferent de limba sistemului (ro-RO ar cere virgulă).
      return double.Parse(line[(eq + 1)..].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
    }
    throw new InvalidOperationException($"EnergyTariff lipsește din {path}");
  }
}
