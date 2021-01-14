namespace Kantoku.Master.Frontend
{
    public interface IConnection
    {
        string Name { get; }

        void Close();
    }
}
