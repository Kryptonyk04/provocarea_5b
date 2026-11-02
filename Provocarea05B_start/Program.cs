// ============================================================================
//  Fabrica TM pune în funcțiune a doua linie de ambalare.
//
//  PARTEA 1 – scenariul unei ture. Puteți modifica DOAR liniile care creează
//             obiectele (new PackagingLine(...)). Operațiile rămân identice.
//  PARTEA 2 – verificările. NU le modificați. Toate trebuie să afișeze OK.
// ============================================================================

// ---------- PARTEA 1: scenariul ----------

PackagingLine line1 = new("Linia 1");   // model PL-120 (maxim 120 pachete/min)
PackagingLine line2 = new("Linia 2");   // model PL-80  (maxim 80 pachete/min)

string b1 = line1.StartBatch();
line1.SetSpeed(110);
line1.Run(30);

string b2 = line2.StartBatch();
line2.SetSpeed(100);                    // operatorul cere 100; PL-80 nu poate
line2.Run(60);

string b3 = line1.StartBatch();         // lot nou pe linia 1
line1.Run(10);

// ---------- PARTEA 2: verificări (NU MODIFICAȚI) ----------
int failures = 0;
double tariff = ReadTariff("plant.config");

Check("1. Linia 1 își păstrează numele", () => line1.Name == "Linia 1");
Check("2. Viteza liniei 1 este 110 pachete/min", () => line1.Speed == 110);
Check("3. Viteza liniei 2 este limitată la 80 pachete/min (PL-80)", () => line2.Speed == 80);
Check("4. Linia 1: 110 × 30 + 110 × 10 = 4400 pachete", () => line1.PackedCount == 4400);
Check("5. Linia 2: 80 × 60 = 4800 pachete", () => line2.PackedCount == 4800);
Check("6. Total fabrică: 9200 pachete", () => PackagingLine.TotalPacked == 9200);
Check("7. Codurile loturilor sunt unice în fabrică",
      () => b1 != b2 && b2 != b3 && b1 != b3);
Check("8. Lotul curent: linia 1 → al treilea lot, linia 2 → al doilea",
      () => line1.CurrentBatch == b3 && line2.CurrentBatch == b2);
Check("9. S-au creat 2 linii", () => PackagingLine.LinesCreated == 2);
Check($"10. Tariful folosit este cel din plant.config ({tariff} lei/kWh)",
      () => PackagingLine.EnergyTariff == tariff);
Check("11. Energia liniei 1: 7,5 kW × 40 min = 5 kWh", () => Math.Abs(line1.EnergyKwh - 5) < 1e-9);
Check("12. Costul energiei liniei 2: 6 kWh × tarif",
      () => Math.Abs(line2.EnergyCost() - 6 * tariff) < 1e-9);

Console.WriteLine(failures == 0 ? "\nToate verificările au trecut." : $"\n{failures} verificări au eșuat.");

void Check(string description, Func<bool> condition)
{
  bool ok;
  try { ok = condition(); }
  catch (Exception) { ok = false; }   // o verificare care aruncă excepție a eșuat
  Console.WriteLine($"{(ok ? "OK " : "BUG")}  {description}");
  if (!ok) failures++;
}

double ReadTariff(string path)
{
  foreach (string line in File.ReadAllLines(path))
  {
    if (line.StartsWith("EnergyTariff="))
      return double.Parse(line["EnergyTariff=".Length..], System.Globalization.CultureInfo.InvariantCulture);
  }
  throw new InvalidOperationException("EnergyTariff lipsește din " + path);
}
