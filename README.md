# Generic-BackgroundWorker

"No more unboxing/casting! Use generic type parameters with this background worker."

A clone of <a href="https://www.codeproject.com/Articles/42103/Generic-Background-Worker">DaveyM69's 'CodeProject'<a/> 
(a WinForms demo-project and a class library)
  
Includes .Net Standard version of the class library (also on Nuget)

Sample usage

```
private void fileWorker_DoWork(object sender, 
        DoWorkEventArgs<string[], List<FileData>> e)
{
    // We're not on the UI thread here so we can't update UI controls directly
    int progress = 0;
    e.Result = new List<filedata>(e.Argument.Length);
    foreach (string file in e.Argument)
    {
        if (fileWorker.CancellationPending)
        {
            e.Cancel = true;
            return;
        }
        fileWorker.ReportProgress(progress, file);
        Thread.Sleep(50);
        e.Result.Add(new FileData(file, DateTime.Now));
        progress += 2;
    }
    fileWorker.ReportProgress(progress, string.Empty);
}

private void fileWorker_ProgressChanged(object sender, 
                        ProgressChangedEventArgs<string> e)
{
    // Back on the UI thread for this
    labelProgress.Text = e.UserState;
    progressBar.Value = e.ProgressPercentage;
}
```
