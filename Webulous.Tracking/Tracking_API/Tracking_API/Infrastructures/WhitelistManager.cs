using Common;
using Tracking_API.Services;

namespace Tracking_API.Infrastructures
{
    public abstract class WhitelistManager<T>
    {
        protected readonly List<T> _whiteList = new();
        protected readonly IFileManager _fileManager;

        protected WhitelistManager(IFileManager fileManager)
        {
            _fileManager = fileManager;

            try
            {
                AddRange(_fileManager.ReadFile());
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        protected abstract T Convert(string input);
        protected abstract bool Matches(T item, string input);
        protected abstract bool Contains(T item, string input);
        protected abstract string Serialize(T item);

        public void Add(string input)
        {
            if (!_whiteList.Any(x => Matches(x, input)))
            {
                _whiteList.Add(Convert(input));
                Save();
            }
        }

        public void AddRange(string[] inputs)
        {
            foreach (var input in inputs)
            {
                Add(input);
            }
        }

        public void Remove(string input)
        {
            _whiteList.RemoveAll(x => Matches(x, input));
            Save();
        }

        public bool IsInSafeList(string input)
        {
            return _whiteList.Any(x => Contains(x, input));
        }

        public string[] GetSafeList()
        {
            return _whiteList.Select(Serialize).ToArray();
        }

        private void Save()
        {
            _fileManager.OverwriteFromList(_whiteList.Select(Serialize).ToList());
        }
    }
}
