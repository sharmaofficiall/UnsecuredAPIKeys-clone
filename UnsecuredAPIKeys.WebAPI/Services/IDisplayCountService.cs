namespace UnsecuredAPIKeys.WebAPI.Services
{
    public interface IDisplayCountService
    {
        long TotalDisplayCount { get; }
        Task InitializeAsync();
        void IncrementCount();
    }
}
