namespace Tracking_API.Model
{
    /// <summary>
    /// Interface utiliser pour faciliter le testing de la classe IPManager avec des mocks.
    /// </summary>
    public interface IFileManager
    {
        string[] ReadFile();
        void OverwriteFromList(List<string> list);
    }
}
