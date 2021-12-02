using System.Runtime.CompilerServices;
[assembly: InternalsVisibleToAttribute("Core")]

var app = App.Workflow();
app.Main(args);

class StackSafety : MIOApp<Unit>
{
    static MIO<Unit> MyProgram = 
        MIO.Succeed(() => 
        {
            Console.WriteLine("Howdy!");
            return Unit();
        }).Repeat(100);

    public MIO<Unit> Run() => MyProgram;
}

class ErrorHandling : MIOApp<Unit>
{
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine(message);
        return Unit();
    });

    static MIO<Unit> MyProgram = 
        MIO.Fail<Unit>(() => new Exception("Failed!"))
            .FlatMap(_ => WriteLine("Here"))
            .CatchAll(ex => WriteLine("Recovered from Error"));

    public MIO<Unit> Run() => MyProgram;
}

class ErrorHandlingThrow : MIOApp<Unit>
{
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine(message);
        return Unit();
    });

    static MIO<Unit> MyProgram = 
        MIO.Succeed<Unit>(() => throw new Exception("Failed!"));

    public MIO<Unit> Run() => MyProgram;
}

class ErrorHandlingThrowCatch : MIOApp<int>
{
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine(message);
        return Unit();
    });

    static MIO<int> MyProgram = 
        MIO
            .Succeed<Unit>(() => throw new Exception("Failed!"))
            .CatchAll(_ => WriteLine("This should never be shown"))
            .FoldCauseMIO(
                c => WriteLine($"Recovered from a cause {c}").ZipRight(() => MIO.Succeed(() => 1)),
                _ => MIO.Succeed(() => 0)
            );

    public MIO<int> Run() => MyProgram;
}

class Async : MIOApp<int>
{
    static MIO<int> AsyncMIO = 
        MIO.Async<int>((complete) => 
        {
            Console.WriteLine("Async Start");
            Thread.Sleep(1000);
            complete(new Random().Next(999));
            Console.WriteLine("Async End");
            return Unit();
        });

    public MIO<int> Run() => AsyncMIO;
}

class AsyncStackSafety : MIOApp<Unit>
{
    static MIO<Unit> MyProgram = 
        MIO.Async<Unit>(complete => 
        {
            Console.WriteLine("Howdy!");
            return complete(Unit());
        }).Repeat(10000);

    public MIO<Unit> Run() => MyProgram;
}

class AsyncStackSafetyFork : MIOApp<Unit>
{
    static MIO<Unit> MyProgram = 
        MIO.Async<Unit>(complete => 
        {
            Console.WriteLine("Howdy!");
            return complete(Unit());
        }).Fork().Repeat(10000);

    public MIO<Unit> Run() => MyProgram;
}

class FlatMap : MIOApp<Unit>
{
    static MIO<(int, string)> ZippedMIO = 
        MIO.Succeed(() => 8).Zip(() => MIO.Succeed(() => "LO"));
    
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine(message);
        return Unit();
    });
    
    static MIO<Unit> MappedMIO = 
        ZippedMIO.FlatMap<Unit>((z) => WriteLine($"My beautiful tuple {z}"));

    public MIO<Unit> Run() => MappedMIO;
}

class Workflow : MIOApp<string>
{
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine(message);
        return Unit();
    });

    static MIO<string> SimpleOne =
        from a in WriteLine($"Nice")
        select "1 nice";

    static MIO<string> SimpleTwo =
        from a in WriteLine($"Nice")
        from b in WriteLine($"Nice")
        select "2 nices";

    static MIO<string> SimpleThree =
        from a in WriteLine($"Nice")
        from b in WriteLine($"Nice")
        from c in WriteLine($"Nice")
        select "3 nices";
        
    static MIO<string> SimpleFour =
        from a in WriteLine($"Nice")
        from b in WriteLine($"Nice")
        from c in WriteLine($"Nice")
        from d in WriteLine($"Nice")
        select "4 nices";

    static MIO<string> SimpleThreeDeSugared =
        WriteLine($"Nice")
            .FlatMap(a => 
                WriteLine($"Nice")
                    .FlatMap(b =>
                        WriteLine($"Nice")
                            .FlatMap(c =>
                                MIO.Succeed(() => "3 nices"))));

    public MIO<string> Run() => SimpleThree;
}

class WorkflowZip : MIOApp<string>
{
    static MIO<(int, string)> ZippedMIO = 
        MIO.Succeed(() => 8).Zip(() => MIO.Succeed(() => "LO"));
    
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine(message);
        return Unit();
    });

    static MIO<string> SimpleOne =
        from a in WriteLine($"Nice")
        select "1 nice";

    static MIO<string> SimpleTwo =
        from a in WriteLine($"Nice")
        from b in WriteLine($"Nice")
        select "2 nices";

    static MIO<string> SimpleThree =
        from a in WriteLine($"Nice")
        from b in WriteLine($"Nice")
        from c in WriteLine($"Nice")
        select "3 nices";

    static MIO<string> SimpleThreeDeSugared =
        WriteLine($"Nice")
            .FlatMap(a => 
                WriteLine($"Nice")
                    .FlatMap(b =>
                        WriteLine($"Nice")
                            .FlatMap(c =>
                                MIO.Succeed(() => "3 nices"))));
    
    static MIO<string> MappedMIO = 
        from z in ZippedMIO
        from _ in WriteLine($"My beautiful tuple {z}")
        select "Nice";

    static MIO<string> MappedMIORaw = 
        ZippedMIO.FlatMap(z => 
            WriteLine($"My beautiful tuple {z}")
                .Map((_) => "Nice"));

    static MIO<string> MappedMIORawAs = 
        ZippedMIO.FlatMap(z => 
            WriteLine($"My beautiful tuple {z}")
                .As("Nice"));

    public MIO<string> Run() => SimpleThree;
}


class Forked : MIOApp<string>
{
    static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
    {
        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {message}");
        return Unit();
    });

    static MIO<int> AsyncMIO = 
        MIO.Async<int>((complete) => 
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Client Start");
            Thread.Sleep(2000);
            complete(new Random().Next(999));
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Client End");
            return Unit();
        });

    static MIO<int> AsyncMIO2 = 
        MIO.Async<int>((complete) => 
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Client Start");
            Thread.Sleep(3000);
            return complete(new Random().Next(999));
        });

    static MIO<string> ForkedMIOOld =
        from fiber1 in AsyncMIO.Fork()
        //from fiber2 in AsyncMIO.Fork()
        from _ in  WriteLine($"Nice")
        //from i1 in fiber1.Join()
        from __ in  WriteLine($"Nice")
        //from i2 in fiber2.Join()
        //select $"My beautiful ints {i1} {i1}";
        select $"My beautiful ints";

    static MIO<string> ForkedMIO =
        from a in  WriteLine($"Nice")
        from b in  WriteLine($"Nice")
        from c in  WriteLine($"Nice")
        //from i2 in fiber2.Join()
        //select $"My beautiful ints {i1} {i1}";
        select $"My beautiful ints";

    public MIO<string> Run() => ForkedMIO;
}

// class Interruption : MIOApp<Unit>
// {
//     static MIO<Unit> WriteLine(string message) => MIO.Succeed(() => 
//     {
//         Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {message}");
//         return Unit();
//     });

//     static MIO<Unit> MyProgram = 
//         from fiber in WriteLine("Howdy!")
//             .Forever()
//             .Ensuring(WriteLine("Goodbye"))
//             .Fork()
//         from sleep in MIO.Succeed(() => 
//         {
//             Thread.Sleep(1000);
//             return Unit();
//         })
//         from _ in fiber.Interrupt()
//         select Unit();

//     public MIO<Unit> Run() => MyProgram;
// }

static class App
{
    public static MIOApp<int> Async() => new Async();
    public static MIOApp<Unit> AsyncStackSafety() => new AsyncStackSafety();
    public static MIOApp<Unit> AsyncStackSafetyFork() => new AsyncStackSafetyFork();
    public static MIOApp<Unit> ErrorHandling() => new ErrorHandling();
    public static MIOApp<Unit> ErrorHandlingThrow() => new ErrorHandlingThrow();
    public static MIOApp<int> ErrorHandlingThrowCatch() => new ErrorHandlingThrowCatch();
    public static MIOApp<string> Forked() => new Forked();
    public static MIOApp<Unit> FlatMap() => new FlatMap();
    public static MIOApp<Unit> StackSafety() => new StackSafety();
    public static MIOApp<string> Workflow() => new Workflow();
}