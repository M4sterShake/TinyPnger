
namespace tinyPnger
{
    interface ProgressHandler
    {
        void SetProgressMax(int max);
        void ResetProgress();
        void SetProgress(int progress);
    }
}
