using FashionFlow.DAL;
using FashionFlow.Presentation.Forms;

namespace FashionFlow.Presentation;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm(new FashionFlowContextFactory()));
    }
}
