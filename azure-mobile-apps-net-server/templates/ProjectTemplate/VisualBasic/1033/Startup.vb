Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(Startup))> 

Partial Public Class Startup
    Public Sub Configuration(ByVal app As IAppBuilder)
        ConfigureMobileApp(app)
    End Sub
End Class