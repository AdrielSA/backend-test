namespace BackendTest.Core.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entidad \"{name}\" ({key}) no encontrada.")
    {
    }
}
