Imports System.Linq
Imports System.Threading.Tasks
Imports System.Web.Http
Imports System.Web.Http.Controllers
Imports System.Web.Http.OData
Imports Microsoft.Azure.Mobile.Server

Public Class TodoItemController
    Inherits TableController(Of TodoItem)

    Protected Overrides Sub Initialize(ByVal controllerContext As HttpControllerContext)
        MyBase.Initialize(controllerContext)
        Dim context As $safecontextclassname$ = New $safecontextclassname$()
        DomainManager = New EntityDomainManager(Of TodoItem)(context, Request)
    End Sub

    ' GET tables/TodoItem
    Public Function GetAllTodoItems() As IQueryable(Of TodoItem)
        Return Query()
    End Function

    ' GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
    Public Function GetTodoItem(ByVal id As String) As SingleResult(Of TodoItem)
        Return Lookup(id)
    End Function

    ' PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
    Public Function PatchTodoItem(ByVal id As String, ByVal patch As Delta(Of TodoItem)) As Task(Of TodoItem)
        Return UpdateAsync(id, patch)
    End Function

    ' POST tables/TodoItem
    Public Async Function PostTodoItem(ByVal item As TodoItem) As Task(Of IHttpActionResult)
        Dim current As TodoItem = Await InsertAsync(item)
        Return CreatedAtRoute("Tables", New With {.id = current.Id}, current)
    End Function

    ' DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
    Public Function DeleteTodoItem(ByVal id As String) As Task
        Return DeleteAsync(id)
    End Function

End Class