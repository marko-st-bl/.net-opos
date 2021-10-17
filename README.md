# .NET-opos

## Zadatak 1 - RASPOREĐIVAČ ZADATAKA

### PRIPREMA

Upoznati se sa razvojnim okruženjem i programskim jezikom C#. Kreirati projekat pod nazivom Zadatak1 unutar
rješenja (i.e. Visual Studio solution) pod nazivom OposZadaci2020, te kreirati projekat pod nazivom Zadatak1.Demo unutar
istog rješenja. Projekat Zadatak1.Demo treba da referencira biblioteku iz projekta Zadatak1 i demonstrira njen rad. U istom
rješenju kreirati projekat Zadatak1.Tests sa jediničnim testovima osnove funkcionalnosti implementirane u zadatku.

### ZADATAK

Korištenjem .NET platforme i programskog jezika C#, unutar dinamički vezane biblioteke implementirati
raspoređivač zadataka, tako da su zadovoljeni sljedeći uslovi.
- Raspoređivač treba da obezbijedi API koji omogućava krajnjem korisniku prosljeđivanje funkcija sa odgovarajućim
nivoom prioriteta. Ovaj API može biti slobodno definisan, ali mora biti dokumentovan.
    - Pored slobodno definisanog API-a, raspoređivač treba da omogući korištenje i korištenjem standardne
osnovne klase za raspoređivače zadataka, System.Threading.Tasks.TaskScheduler, odnosno
raspoređivač mora da bude ispravno implementirana izvedena klasa iz bazne klase raspoređivača.
- Omogućiti specifikaciju broja niti kojima raspoređivač raspolaže.
- Omogućiti preventivno i nepreventivno raspoređivanje.
    - Implementacija preventivnog raspoređivanja treba da specifikuje API kog funkcije koje se raspoređuju
treba da koriste za svrhu zaključavanja (e.g. PIP ili PCP).
    - Obezbijediti detekciju deadlock situacija i njihovo elementarno razrješavanje.
- Omogućiti specifikaciju roka izvršenja svih zadataka proslijeđenih raspoređivaču.
    - Omogućiti specifikaciju ograničenja vremena izvršavanja za svaku od funkcija proslijeđenih raspoređivaču
na raspoređivanje.
- Omogućiti raspoređivanje u realnom vremenu (raspoređivanje funkcija za vrijeme dok raspoređivač izvršava
prethodno raspoređene funkcije).
- Dokumentovati način pozivanja svih javnih metoda kroz odgovarajuće dokumentacione komentare, te obezbijediti
dokumentaciju u obliku markdown fajla uz primjere upotrebe raspoređivača u odgovarajućem projektu.
- Implementirati jednostavne jedinične testove u odgovarajućem projektu. Jedinične testove ne treba duplirati u
projektu za demonstraciju.
Pri implementaciji zadatka neophodno je, kao dodatnu funkcionalnost, implementirati jednu od sljedećih stavki.
- Specifikacija načina razrješavanja deadlock situacija.
- Mehanizam za sinhronizaciju između zadataka omogućen kroz slobodno definisani API.
- Mehanizam za pristup dijeljenim resursima omogućen kroz slobodno definisani API.

## ZADATAK 2 – IZOLOVANA APLIKACIJA ZA PARALELNI PRORAČUN

### PRIPREMA
Kreirati projekat pod nazivom Zadatak2 unutar rješenja pod nazivom OposZadaci2020, te kreirati projekat pod
nazivom Zadatak2.Demo unutar istog rješenja. Projekat Zadatak2.Demo treba da referencira biblioteku iz projekta Zadatak2
i demonstrira njen rad. U istom rješenju kreirati projekat Zadatak2.Tests sa jediničnim testovima osnove funkcionalnosti
implementirane u zadatku. Alternativno, konstruisati ekvivalentan projekat i rješenje za rad sa Android platformom, uz istu
konvenciju imenovanja.
### ZADATAK
Kao izolovanu aplikaciju (UWP ili Android), implementirati korisnički interfejs i sistem za paralelno obavljanje i
raspoređivanje nekog posla, i to uz sljedeća ograničenja.
- Posao koji se raspoređuje treba da zadovolji sljedeća ograničenja.
    - Posao mora moći da bude ubrzan paralelizacijom.
    - Posao mora moći da se učita iz nekog fajla koji ga specifikuje.
    - Posao mora zahtijevati upis rezultata u fajl na fajl-sistemu ili slanje rezultata kroz mrežu ili drugoj aplikaciji.
- Obezbijediti korisnički interfejs za raspoređivanje više poslova paralelno, te obezbijediti prijavu toka obavljanja
posla korištenjem odgovarajuće ProgressBar komponente. Potrebno je ažurirati stanje posla redovno i bez
remećenja korisničkog iskustva (UI treba da ostane responizvan).
- Omogućiti ograničavanje broja jezgara po poslu i ograničavanje broja paralelnih poslova.
- Omogućiti zaustavljanje, pauziranje i nastavak posla.
- Obezbijediti učitavanje posla iz fajla, kao i čuvanje rezultata proračuna u fajl.
- Implementirati pozadinski zadatak koji obavještava korisnika o nezavršenim poslovima.
- Obezbijediti perzistenciju posla nakon zatvaranja i ponovnog otvaranja aplikacije.
- Obezbijediti odgovarajuće dijalog-prozore sa obavještenjima u slučajevima kada dolazi do greške.
Pri implementaciji zadatka neophodno je, kao dodatnu funkcionalnost, implementirati jednu od sljedećih stavki.
- Korištenje kamere za akviziciju nekog parametra posla (e.g. ukoliko aplikacija vrši obradu slike)
- Korištenje mikrofona za akviziciju nekog parametra posla (e.g. ukoliko aplikacija vrši obradu zvuka)
- Korištenje senzora za akviziciju nekog parametra posla (e.g. ukoliko aplikacija radi sa mapama ili GPS sistemom)
- Registraciju za rad sa specifičnom ekstenzijom fajla, kako bi se posao mogao učitati dvoklikom na fajl.

## ZADATAK 3 –IGRA ŽIVOTA (CONWAY)

### PRIPREMA

Kreirati projekat pod nazivom Zadatak3 unutar rješenja pod nazivom OposZadaci2020, te kreirati projekat pod
nazivom Zadatak3.Demo unutar istog rješenja. Projekat Zadatak3.Demo treba da referencira biblioteku iz projekta Zadatak3
i demonstrira njen rad. Dozvoljeno je korištenje pomoćnih projekata (e.g. wrapper biblioteka za upotrebu osnovne biblioteke
iz projekta Zadatak3, iz nekog drugog jezika). Projekat Zadatak3.Demo može da bude implementiran kao aplikacija sa
korisničkim interfejsom ili kao konzolna aplikacija.

### ZADATAK

Korištenjem OpenCL ili CUDA platforme, implementirati biblioteku za simuliranje koraka Conway igre života.
Biblioteka treba da obezbjeđuje sljedeću funkcionalnost. Neophodno je implementirati samo jednu od dvije naznačene
dodatne funkcionalnosti.
- Predstavljanje dvodimenzionog prostora igre korištenjem binarne matrice proizvoljne veličine.
- Dohvatanje nekog podsegmenta ograničenog dvodimenzionog prostora kao crno-bijele slike, u kojoj bijeli pikseli
označavaju živa polja.
- Omogućiti ručno podešavanje inicijalnih parametara simulacije (inicijalne položaje živih ćelija).
    - Omogućiti podešavanje podsegmenata dvodimenzionog prostora igre prosljeđivanjem odgovarajuće slike.
- Omogućiti prelazak na proizvoljnu iteraciju igre, počevši od trenutnog stanja igre.
- Dodatna funkcionalnost 1: Omogućiti prelazak na sljedeću iteraciju igre uz detekciju jednog oscilatorskog obrasca i
uz njegovo označavanje posebnom bojom u trokanalnoj dobavljenoj slici.
Pri implementaciji zadatka neophodno je koristiti paralelizaciju na GPGPU, i to uz sljedeća ograničenja.
- Svaka od prethodnih stavki treba da bude implementirana kao zaseban kernel.
- U slučajevima kada se simulira značajno velik prostor igre, podijeliti posao u nekoliko pokretanja kernela.
- Prostor za igru treba da bude alociran na uređaju.
- Iako Igra života podrazumijeva beskonačan prostor, prihvatljivo je alocirati kvadratnu matricu dimenzionalnosti
koju je moguće alocirati na postojećem grafičkom uređaju. Rubni slučajevi mogu da se obrađuju na proizvoljan način
(e.g. odbacivanje rubova ili povezivanje desne i lijeve ivice i gornje i donje ivice, odnosno teselacija prostora).
- Pri alokaciji prostora, voditi računa da je neophodno čuvati prethodno i trenutnu iteraciju, jer se trenutna iteracija
izračunava na osnovu polja u prethodnoj.
- U slučaju prve dodatne funkcionalnosti, dobavljanje naredne iteracije i detekcija oscilatorskog obrasca treba da
budu izvedeni jednim kernelom. Preporučuje se korištenje pomoćnih funkcija, radi izbjegavanja dupliranja koda.
- Dodatna funkcionalnost 2: Radi minimizacije upotrebljenog memorijskog prostora, ćelije prostora čuvati na nivou
bita (i.e. izvršiti bitsko pakovanje ćelija, tako da se u jednom bajtu mreže čuva osam horizontalnih ćelija).

### PRAVILA IGRE

Kao primjer aplikacije služi sljedeća ekspanzija opisa.
- Univerzum igre predstavlja dvodimenziona mreža kvadratnih ćelija koje mogu da postoje u dva stanja: živo i neživo.
Svaka ćelija interaguje sa svojih osam susjeda u mreži (odnosno, sa dva horizontalna, dva vertikalna i četiri
dijagonalna susjeda).
- U svakoj iteraciji simulacije, izvršavaju se sljedeće transakcije.
    - Svaka živa ćelija sa manje od dva živa susjeda postaje neživa.
    - Svaka živa ćelija sa dva ili više živih susjeda prenosi se u sljedeću iteraciju.
    - Svaka živa ćelija sa više od tri živa susjeda postaje neživa.
    - Svaka neživa ćelija sa tačno tri živa susjeda postaje živa.

## ZADATAK 4 – FAJL-SISTEM MAPIRAN U MEMORIJI

### PRIPREMA

Kreirati projekat pod nazivom Zadatak4 unutar rješenja pod nazivom OposZadaci2020. Projekat Zadatak4 treba da
sadrži implementaciju rješenja za fajl-sistem u korisničkom prostoru.

### ZADATAK

Korištenjem Dokan ili FUSE frejmvorka i odgovarajućeg programskog jezika, implementirati fajl-sistem drajver u
korisničkom prostoru, tako da su zadovoljena sljedeća ograničenja.
- Fajl-sistem treba da čuva fajlove u memoriji i predstavlja se operativnom sistemu kao standardni disk-uređaj.
- Fajlovi treba da se čuvaju u hijerarhijskom poretku korištenjem B-stabla.
- Omogućiti osnovne operacije rada sa fajlovima.
    - Omogućiti dobavljanje osnovnih informacija o fajlovima (datum kreiranja, naziv i veličina).
    - Omogućiti dodavanje fajlova u proizvoljni direktorijum.
    - Omogućiti brisanje fajlova.
    - Onemogućiti izmjenu fajlova.
    - Spriječiti slučajeve utrkivanja.
- Obezbijediti postojanje posebne putanje takve da pristup njoj vraća binarni fajl sa serijalizovanim sadržajem cijelog
čuvanog B-stabla.