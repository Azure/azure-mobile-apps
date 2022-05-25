Imports System.Web.Http
Imports Microsoft.Azure.Mobile.Server.Config

' Use the MobileAppController attribute for each ApiController you want to use  
' from your mobile clients 
<MobileAppController()>
Public Class ValuesController
    Inherits ApiController

    ' GET api/values
    Public Function GetValue() As String
        Return "Hello World!"
    End Function

    ' POST api/values
    Public Function PostValue() As String
        Return "Hello World!"
    End Function
End Class