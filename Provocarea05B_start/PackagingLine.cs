// Linie de ambalare (cutii de carton) – fabrica TM.
// Scrisă când fabrica avea o singură linie: „oricum e una singură, fac totul static".
// Ulterior cineva a adăugat un constructor, ca să „arate a obiect".
//
// Modelul PL-120: viteză maximă 120 pachete/min, putere 2 kW + 0,05 kW pentru fiecare pachet/min.
public class PackagingLine
{
  public const string PlantCode = "TM";             // codul fabricii
  public const double EnergyTariff = 0.92;          // lei/kWh – „nu se schimbă"
  public const int MaxSpeed = 120;                  // pachete/min (modelul PL-120)

  private static string name = "";
  private static int speed;                         // pachete/min
  private static int packedCount;
  private static double energyKwh;
  private static string currentBatch = "";
  private static int nextBatchNumber = 1;
  private static int linesCreated;

  public PackagingLine(string lineName)
  {
    name = lineName;
    linesCreated++;
  }

  public string Name => name;
  public int Speed => speed;
  public int PackedCount => packedCount;
  public double EnergyKwh => energyKwh;
  public string CurrentBatch => currentBatch;

  public static int LinesCreated => linesCreated;
  public static int TotalPacked => packedCount;

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
    packedCount += speed * minutes;
    double powerKw = 2 + 0.05 * speed;
    energyKwh += powerKw * minutes / 60.0;
  }

  public double EnergyCost()
  {
    return energyKwh * EnergyTariff;
  }
}
