# MyOposScheduler

MyOposScheduler pruža svojim korisnicima API za raspoređivanje zadataka na osnovu prioriteta. Moguce je sprecifikovati broj niti kojima rasporedjivac raspolaze.
Podržava dva načina raspoređivanja:
- Preventivno (*Preeemptive*)
- Nepreventivno (*Non-Preemptive*) 


# 1. Upotreba

## 1.1 Pisanje funkcija za rasporedjivanje
Zadaci koji se rasporedjuju moraju biti proslijedjeni rasporedjivaacu preko delegata ***NewTaskStart(TaskController)***. Upravljanje zadacima vrsi se kooperativno, odnosno programer koji pise funkcije koje se rasporedjuju mora postovati specifikaciju rasporedjivaca da bi on ispravno radio:
- Funcije moraju imati povratni tip ***void*** i primati parametar tipa ***TaskController***, odnosno:
  ```c#
  void Example(DataReporter dataReporter){}
  ```
- U toku izvrsavanja funkcije neophodno je cesto provjeravati token za zaustavljanje pomocu atributa ***IsCanceled*** klase TaskController i ukoliko je on setovan duznost programera je da obezbjedi zaustavljanje zadatka u najkracem mogucem roku. Kod funkcije koje izvrsavaju neke petlje preporuceno je provjeravati token nakon svake iteracije. Dok je preporuka da se funkcije koje ne izvrsavaju petlje provjera tokena obavlja poslije svake logicke cjeline.
- U slucaju upotrebe preventivnog zaustavljanja, pored *IsCancaled* tokena slicno je neophodno provjeravati i ***IsPaused*** token, s razlikom u tome sto se to vrsi na sledeci nacin:
    ```c#
    lock(taskController._locker)
    {
        while (taskController.IsPaused)
        {
            Monitor.Wait(taskController._locker);
        } 
    }
    ```
- Za ulazak i izlazak iz kriticne sekcije zadatka, neophodno je korisiti metode ***Enter(object locket)*** i ***Exit(object locker)*** klase *TaskController* respektivno, da bi rasporedjivac mogao detektovati i potencijalno razrijesiti slucajeve ***deadlocka***.

## 1.2. Konstruktor
Klasa MyOposScheduler ima jedan konstruktor ciji je potpis:
```c#
public MyOposScheduler(int maxParallelThreads, SchedulingMode mode=SchedulingMode.NonPreemtive)
```
## 1.3. Metode
- ***public void ScheduleTask(NewTaskStart task, int priority, int maxDuration)***
    Metoda koja se koristi za prosljedjivanje zadataka rasporedjivacu.
## 1.4. Polja
- ***public int MaxParallelTask*** - predstavlja broj zadataka koji se mogu istovremeno izvrsavati
- ***public int CurrentTaskCount*** - predstavlja broj zadataka koji se trenutno izvrsavaju.
# 2. Primjeri
## 2.1. Nepreventivno rasporedjivanje
Navedeni primjer ilustruje nepreventivno rasporedjivanje 15 zadataka koji imaju prioritete od 0-9, gdje 0 predstavlja najnizi a 9 najvisi prioritet.
```c#
void printf(TaskController taskController, int maxDuration, int val)
{
    for (int i=0; i<maxDuration; i++)
    {
        // Ako je token IsCancelled setovan prekidamo izvrsavanje zadatka
        if (taskController.IsCancelled)
        {
            Console.WriteLine("Task {0} canceled after {1} seconds", val, i);
            break;
        }
        Console.WriteLine("Task {0} is executing... i={1}", val, i);
        // Simulira neki posao
        Task.Delay(1000).Wait();
        if (i == (maxDuration - 1))
            Console.WriteLine("Task {0} Done!", val);
    }               
}

const int numOfTasks = 15;
// Kreira rasporedjivac koji ima dostupne dvije niti i radi u Nepreventivnom nacinu rasporedjivanja
MyOposScheduler osch = new MyOposScheduler(2, MyOposScheduler.SchedulingMode.NonPreemptive);
NewTaskStart[] tasks = new NewTaskStart[numOfTasks];

for (int i = 0; i < numOfTasks; i++)
{
    int val = i;
    tasks[i] = x => printf(x, 3, val, cts.Token;
    // Rasporedjuje zadatke sa prioritetom 0-9 sa maksimalnim vremenom izvrsavanja od 5 sekundi
    osch.ScheduleTask(tasks[i], i % 10, 5000);
}
// Cekamo dok svi zadaci ne zavrse
 while (osch.CurrentTaskCount > 0)
    Task.Delay(1000).Wait();
```
## 2.2. Preventivno rasporedjivanje upotrebom standardne klase za rasporedjivanje
Ovaj primjer ilustruje upotrebu rasporedjivaca koji rasporedjuje 15 zadataka Preventivno upotrebom standardne klase za rasporedjivanje zadataka *System.Threading.Tasks.TaskScheduler*. S obzirom da klasa Task ne prima parametar za prioritet, da bi koristili ovaj rasporedjivac umjesto klase **Task** morate koristiti klasu ***OposTask***, koja nasljedjuje klasu *Task* s razlikom sto ima tri dodatna polja:
- ***Priority*** -tipa int, sluzi za specifikovanje prioriteta zadatka
- ***MaxDuration*** - tipa int, specifikuje maksimalno vrijeme izvrsavanja zadatka u ***ms***.
- ***nts*** (**N**ew**T**ask**S**tart) - ranije pomenuti delegat za prosljedjivanje korisnicke funkcije.

Procedura pokretanja zadatka je sledeca:
1. Kreiramo instancu klase *OposTask* (isto kao i *Task*)
2. Specifikujemo parametre
    - Priority
    - MaxDuration
    - nfs
3. Pozovemo metodu ***Start(myOposScheduler)*** kojoj kao paramater proslijedimo instancu naseg rasporedjivaca - *OposScheduler*;

Kompletan primjer:
```c#
void printf(TaskController taskController, int maxDuration, int val)
    {
        for (int i=0; i<maxDuration; i++)
        {
            lock (taskController._locker)
            {
                while (taskController.IsPaused)
                {
                    Console.WriteLine("Task-{0} is WAITING.", val);
                    Monitor.Wait(taskController._locker);
                    if(taskController.IsCancelled)
                    {
                        taskController.Exit(locker); 
                    }
                }
            }
            if (taskController.IsCancelled)
            {
                Console.WriteLine("Task {0} canceled after {1} seconds", val, i);
                break;
            }
            Console.WriteLine("Task {0} is executing... i={1}", val, i);
            Task.Delay(1000).Wait();
            if (i == (maxDuration - 1))
                Console.WriteLine("Task {0} Done!", val);
        }
        
    }
    
    const int numOfTasks = 5;
    MyOposScheduler osch = new MyOposScheduler(4, MyOposScheduler.SchedulingMode.Preemptive);
    Console.WriteLine(osch.Mode);
    MyOposScheduler.NewTaskStart[] tasks = new MyOposScheduler.NewTaskStart[numOfTasks];

    for (int i = 0; i < numOfTasks; i++)
    {
        int val = i;
        OposTask otask = new OposTask(() => { });
        otask.Priority = i % 10;
        otask.MaxDuration = 5000;
        otask.nts = x => printf(x, 3, val * 2);
        otask.Start(osch);
    }
    // Cekamo da svi zadaci zavrse sa izvrsavanjem
    while (osch.CurrentTaskCount > 0)
        Task.Delay(1000).Wait();
        
```
## 2.3. Preventivno rasporedjivanje sa razrjesavanjem *deadlocka*
Slijedeci primjer ilustruje rasporedjivanje tri zadatka, od kojih dva zadatka zakljucavaju isti resurs. Za razrjesavanje *deadlocka MyOposScheduler* koristi **PIP**  (**P**riority **I**nheritance **P**rotocol) algoritam. Pa ce se u navadenom primjeru zadatku 1 ce se dodijeliti prioritet zadatka 2 da bi se izbjegla inverzija prioriteta.
```c#
/// Resurs koji zakljucavaju zadaci 1 i 3
object locker = new object();
// funkcija koja ne koristi resurs
void printf(TaskController taskController, int maxDuration, int val)
{
    for (int i=0; i<maxDuration; i++)
    {
        lock (taskController._locker)
        {
            while (taskController.IsPaused)
            {
                Console.WriteLine("Task-{0} is WAITING.", val);
                Monitor.Wait(taskController._locker);
                if(taskController.IsCancelled)
                {
                    taskController.Exit(locker); 
                }
            }
        }
        if (taskController.IsCancelled)
        {
            Console.WriteLine("Task {0} canceled after {1} seconds", val, i);
            break;
        }
        Console.WriteLine("Task {0} is executing... i={1}", val, i);
        Task.Delay(1000).Wait();
        if (i == (maxDuration - 1))
            Console.WriteLine("Task {0} Done!", val);
    }
    
}
// funkcija koja koristi resurs - locker
void printf1(TaskController taskController, int maxDuration, int val)
{
    // Za zakljucavanje koristiti funkciju Enter klase TaskController
    taskController.Enter(locker);
    for (int i = 0; i < maxDuration; i++)
    {
        lock (taskController._locker)
        {
            Console.WriteLine("Task {0} is executing... i={1}", val, i);
            while (taskController.IsPaused)
            {
                taskController.Wait();
            }
        }
        if (taskController.IsCancelled)
        {
            Console.WriteLine("Task {0} canceled after {1} seconds", val, i);
            taskController.Exit(locker);
            break;
        }
        Task.Delay(1000).Wait();
        if (i == (maxDuration - 1))
            Console.WriteLine("Task {0} Done!", val);
    }
    // Za oslobadjanje Resursa koristiti metodu Exit klase TaskController
    taskController.Exit(locker);

}
CancellationTokenSource cts = new CancellationTokenSource();
const int numOfTasks = 3;
MyOposScheduler osch = new MyOposScheduler(1, MyOposScheduler.SchedulingMode.Preemptive);
Console.WriteLine(osch.Mode);
MyOposScheduler.NewTaskStart[] tasks = new MyOposScheduler.NewTaskStart[numOfTasks];

tasks[0] = x => printf1(x, 3, 1);
tasks[1] = x => printf1(x, 3, 2);
tasks[2] = x => printf(x, 3, 3);
osch.ScheduleTask(tasks[0], 1, 10000);
osch.ScheduleTask(tasks[1], 9, 10000);
osch.ScheduleTask(tasks[2], 4, 10000);

while (osch.CurrentTaskCount > 0)
    Task.Delay(1000).Wait();

    // IZLAZ

    // Preemptive
    // Task dequed with priority: 1
    // Task 1 executing...i=0
    // Task with priority 9 paused task with priority 1
    // ERROR: DEADLOCK DETECTED: Task with priority 1 blocks task with priority 9
    // Task 1 is executing...i=1
    // Task 1 is executing...i=2
    // Task 1 done!
    // Task with priority: 9 RESUMED
    // Task 2 is executing...i=0
    // Task 2 is executing...i=1
    // Task 2 is executing...i=2
    // Task dequed with priority: 4
    // Task 3 is executing...i=0
    // Task 3 is executing...i=1
    // Task 3 is executing...i=2
    // Task 3 done!
```
Autor: Marko Stojanovic 11159/16
  