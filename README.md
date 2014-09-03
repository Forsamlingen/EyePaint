#EyePaint
## Om programmet
EyePaint (sv: Måla med ögonen) är ett Windows program skrivet i .NET som tillåter användaren att måla på skärmen med ögonen, genom att använda en [Tobii EyeX](http://www.tobii.com/en/eye-experience/) eye tracker.

## Ladda ned programmet
Den senaste versionen finns [här](https://github.com/Forsamlingen/EyePaint/releases/latest).

## Hårdvarukrav
 - Att kontinuerligt generera bitmap bilder i presentationsramverket som programmet använder är krävande och förutsätter att datorn har en kraftfull processor. Processorer i serierna Intel Core i7 eller Intel Core i5 är vettiga alternativ. [Lämpliga processorer](http://www.cpubenchmark.net/high_end_cpus.html).
 - Tobiis nya eye tracker (EyeX) är mycket bättre än den äldre modellen (REX). Dock kräver den nya eye trackern en dator med en USB 3.0 port. Den äldre modellen hade problem med att den kunde sluta fungera med förlängningskablar och att den var svårare att montera.
 - Skärmupplösningen bör varken överstiga eller understiga [Full HD](https://en.wikipedia.org/wiki/1080p).
 - Programmet kräver en internetuppkoppling för att kunna skicka felrapporter och för att kunna ladda upp användarens målningar. Dessutom krävs en SMTP server (ingår vanligtvis i ett bredbandsavtal och leverantören brukar lista serveradressen på sin webbplats).
 - Tillgänglighet: 
  - Om användaren inte lyckas fokusera på ritytan kan den inte börja måla med ögonen. Därför är det lämpligt om det finns en hårdvaruknapp eller kontaktyta som aktiverar målandet vid beröring. Komponenten bör skicka en vanlig vänstermusklicksignal till datorn.
  - Om användaren inte lyckas fokusera på knapparna i användargränssnittet är det lämpligt om användaren kan peka och klicka på en knapp med fingret istället, således är en pekskärm ett lämpligt hjälpmedel.
  

**Ovanstående krav bör tas hänsyn till för en optimal användarupplevelse. Förslagsvis används en allt-i-ett PC med pekskärm: [exempel](https://www.dustin.se/product/5010751515/eliteone-800-g1).**

## Installationsinstruktioner
1. Installera [Tobii EyeX](http://developer.tobii.com/eyex-setup) och följ instruktionerna.
1. Genomför en grundkalibrering med en typisk användare (ej glasögon eller linser). Kalibreringen bör göras om utifall eye trackern flyttas på.
1. Stäng av den grafiska ögonindikatorn i Tobii EyeX kontrollpanelen.
1. Installera [den senste versionen av programmet](https://github.com/Forsamlingen/EyePaint/releases/latest).
1. Programmet behöver konfigureras. Starta programmet och klicka in i inställningsmenyn. Fyll i fälten och spara. Konfigurationen behöver bara göras en gång per installation.
1. För att avsluta programmet manuellt gäller det att använda Aktivitetshanteraren (öppnas med `CTRL+SHIFT+ESC`) och hitta programmet i listan (`EyePaint.exe`) och trycka `DEL`. Tips: eye trackern tar över muspekaren men datormusen återfår kontrollen om en hand läggs över eye trackern.
1. Kom ihåg att konfigurera operativsystemet också, såsom att stänga av Windows Update, skärmsläckaren, sovlägen m.m., samt se till att programmet autostartar med Windows, och att datorn autostartar vid strömavbrott (vanligtvis en BIOS inställning).
