namespace backendWpf;

public partial class MainWindow
{
  private void TestBackend()
  {
    try
    {
      var reply = _api.ValuesPasswordsGet();
      Title = $"IsOk={reply.IsOk} / Nr={reply.Nr}";
    }
    catch (Exception ex)
    {
      Title = ex.Message;
    }
  }
}

