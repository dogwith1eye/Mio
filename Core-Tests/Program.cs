
var app = APP.ErrorHandling();
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

static class APP
{
    public static MIOApp<Unit> ErrorHandling() => new ErrorHandling();
    public static MIOApp<Unit> StackSafety() => new StackSafety();
}