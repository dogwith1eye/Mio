
var app = App.ErrorHandlingThrowCatch();
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

static class App
{
    public static MIOApp<Unit> ErrorHandling() => new ErrorHandling();
    public static MIOApp<Unit> ErrorHandlingThrow() => new ErrorHandlingThrow();
    public static MIOApp<int> ErrorHandlingThrowCatch() => new ErrorHandlingThrowCatch();
    public static MIOApp<Unit> StackSafety() => new StackSafety();
}