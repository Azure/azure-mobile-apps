Imports System.Data.Entity
Imports System.Data.Entity.ModelConfiguration.Conventions
Imports System.Linq
Imports Microsoft.Azure.Mobile.Server
Imports Microsoft.Azure.Mobile.Server.Tables


Public Class $safecontextclassname$
    Inherits DbContext

    ' You can add custom code to this file. Changes will not be overwritten.
    '
    ' If you want Entity Framework to alter your database
    ' automatically whenever you change your model schema, please use data migrations.
    ' For more information refer to the documentation:
    ' http://msdn.microsoft.com/en-us/data/jj591621.aspx
    '
    ' To enable Entity Framework migrations in the cloud, please ensure that the 
    ' service name, set by the MS_MobileServiceName AppSettings in the local 
    ' Web.config, is the same as the service name when hosted in Azure.

    Private Const connectionStringName As String = "Name=MS_TableConnectionString"

    Public Sub New()
        MyBase.New(connectionStringName)
    End Sub

    Public Property TodoItems As DbSet(Of TodoItem)

    Protected Overrides Sub OnModelCreating(ByVal modelBuilder As DbModelBuilder)
        modelBuilder.Conventions.Add(
            New AttributeToColumnAnnotationConvention(Of TableColumnAttribute, String)(
                "ServiceTableColumn", Function(prop, attributes) attributes.Single().ColumnType.ToString()))

    End Sub
End Class
