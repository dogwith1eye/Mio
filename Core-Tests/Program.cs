
var app = App.ErrorHandlingThrow();
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

static class App
{
    public static MIOApp<Unit> ErrorHandling() => new ErrorHandling();
    public static MIOApp<Unit> ErrorHandlingThrow() => new ErrorHandlingThrow();
    public static MIOApp<Unit> StackSafety() => new StackSafety();
}